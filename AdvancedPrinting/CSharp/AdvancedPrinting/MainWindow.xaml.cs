using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Geometry;
using devDept.Graphics;

using System.Windows.Forms;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using RectangleF = System.Drawing.RectangleF;
using Rectangle = System.Drawing.Rectangle;
using Font = System.Drawing.Font;
using Bitmap = System.Drawing.Bitmap;
using Environment = devDept.Eyeshot.Environment;
using Size = System.Drawing.Size;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Camera secondCamera;

        private MyHiddenLinesViewPrint hdlView1, hdlView2;

        // Pens used to draw the lines
        private Pen PenEdge, PenSilho, PenWire;

        private System.Drawing.Printing.PrintDocument printDocument1;

        private Font Font;

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

            Font = new Font("FontFamily", 14, System.Drawing.FontStyle.Regular);
            printDocument1 = new System.Drawing.Printing.PrintDocument();
            printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(printDocument1_PrintPage);
        }

        protected override void OnContentRendered(EventArgs e)
        {            
        
            // Creates the pens
            PenSilho = new Pen(System.Drawing.Color.Black, 3.0f);
            PenEdge = new Pen(System.Drawing.Color.Black, 1.0f);
            PenWire = new Pen(System.Drawing.Color.Black, 1.0f);    
            PenEdge.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
            PenSilho.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
            PenWire.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);             


            model1.GetGrid().AutoSize = true;
            model1.GetGrid().Step = 50;
            model1.Camera.FocalLength = 30;
            model1.Camera.ProjectionMode = projectionType.Perspective;            
            model1.SetView(viewType.Trimetric);            
            model1.ZoomFit();
            model1.Invalidate();

            // Imports an Ascii model
            devDept.Eyeshot.Translators.ReadFile rf = new devDept.Eyeshot.Translators.ReadFile("../../../../../../dataset/Assets/house.eye");
            rf.DoWork();
            model1.Entities.AddRange(rf.Entities, Color.Gray);

            // Changes the color/material of the fifth entity            
            rf.Entities[5].Color = System.Drawing.Color.Pink;
            
            model1.ZoomFit();
            comboBoxPrintMode.SelectedIndex = 0;
         
            base.OnContentRendered(e);
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            // Defines the camera for the second view                        
            secondCamera = new Camera(new Point3D(320, 0, 160),
                                           600,
                                           new Quaternion(Vector3D.AxisZ, 90),
                                           projectionType.Orthographic,
                                           50,
                                           1);

            if (comboBoxPrintMode.SelectedIndex == 0) // Vector printing
            {                
                hdlView1 = new MyHiddenLinesViewPrint(new HiddenLinesViewSettings(model1.Viewports[0], model1, 0.1, true, PenSilho, PenEdge, PenWire, false));
                model1.StartWork(hdlView1);
            }
            else // Raster printing
            {
                secondCamera.Move(50, 50, 0);

                // Prints the page
                Print();
            }
        }

        private void model1_WorkCompleted(object sender, WorkCompletedEventArgs e)
        {
            if (e.WorkUnit == hdlView1)
            {
                var prevCam = model1.Viewports[0].Camera;

                model1.Viewports[0].Camera = secondCamera;

                hdlView2 = new MyHiddenLinesViewPrint(new HiddenLinesViewSettings(model1.Viewports[0], model1, 0.1, PenSilho, PenEdge, PenWire, false));

                model1.Viewports[0].Camera = prevCam;

                // Runs the hidden lines computation for the second view
                model1.StartWork(hdlView2);
            }
            else if (e.WorkUnit == hdlView2)
            {
                // Prints the page
                Print();
            }

        }

        public void Print()
        {
            PrintPreviewDialog ppDlg = new PrintPreviewDialog();

            ppDlg.Document = printDocument1;

            // Sets the property to true to have the drawing correctly centered on the page
            printDocument1.OriginAtMargins = true;

            try
            {
                ppDlg.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            RectangleF printable = e.MarginBounds;

            // Since PrintDocument.OriginAtMargins = True, sets top-Left corner to (0,0)
            printable.X = 0;
            printable.Y = 0;

            // Draws the logo
            int logoSize = 70;
            e.Graphics.DrawImage(Properties.Resources.logo, printable.Right - logoSize, printable.Y, logoSize, logoSize);

            //////////////
            // Draws the main title and some text
            //////////////

            string title = "Advanced Printing sample";

            Font titleFont = new Font(Font.FontFamily, 30, System.Drawing.FontStyle.Regular);

            System.Drawing.SizeF stringSize = e.Graphics.MeasureString(title, titleFont);

            float nextY = printable.Top + logoSize / 2;

            e.Graphics.DrawString(title, titleFont, System.Drawing.Brushes.Blue, printable.Left + printable.Width / 2 - stringSize.Width / 2, nextY);

            int verticalOffset = 60;

            nextY += stringSize.Height + 20;

            Font textFont = new Font(Font.FontFamily, 12, System.Drawing.FontStyle.Regular);

            string text = "This sample demonstrates how to draw different views of the same model in the proper page area.";
            stringSize = e.Graphics.MeasureString(text, textFont);

            e.Graphics.DrawString(text, textFont, System.Drawing.Brushes.Black, new RectangleF(printable.X, nextY, printable.Width, printable.Height));

            nextY += stringSize.Height + verticalOffset;

            // Defines a margin
            int marginFromBorder = 5;

            // Draw the views
            if (comboBoxPrintMode.SelectedIndex == 0) // Vector
                PrintPageVector(sender, e, ref nextY, verticalOffset, printable, marginFromBorder);
            else // Raster
                PrintPageRaster(sender, e, ref nextY, verticalOffset, printable, marginFromBorder);

            //////////////
            // Draws some other text
            //////////////

            Font titleFont2 = new Font(Font.FontFamily, 20, System.Drawing.FontStyle.Bold);
            string title2 = "Window opening details";

            System.Drawing.SizeF title2Size = e.Graphics.MeasureString(title2, titleFont2);

            e.Graphics.DrawString(title2, titleFont2, System.Drawing.Brushes.Blue, printable.Left + printable.Width / 2 - title2Size.Width / 2, nextY + verticalOffset);

            nextY += 2 * (marginFromBorder + verticalOffset);

            text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            e.Graphics.DrawString(text, textFont, System.Drawing.Brushes.Black, new RectangleF(printable.Left, nextY, printable.Width / 2 - 20, printable.Height - nextY));
        }

        private void pageSetupButton_Click(object sender, RoutedEventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();

            pageSetupDialog.Document = printDocument1;
            pageSetupDialog.AllowMargins = true;

            if (pageSetupDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)

                printDocument1 = pageSetupDialog.Document;
        }

        private void exportToEMFButton_Click(object sender, RoutedEventArgs e)
        {            
            // It is possible to save in DWG / DXF with the HiddenLinesViewOnFileAutodesk class available in x86 and x64 dlls

            model1.WriteToFileVector(true, "house.emf");
        }

        private void PrintPageVector(object sender, System.Drawing.Printing.PrintPageEventArgs e,
                                     ref float nextY, int verticalOffset, RectangleF printable, int marginFromBorder)
        {
            //////////////
            // First View
            //////////////

            hdlView1.PrintRect = new RectangleF(printable.Left, nextY, printable.Width - 20, 200);

            // Draws the first view
            DrawViewFrame(e.Graphics, "First View", hdlView1.PrintRect, marginFromBorder);

            hdlView1.Print(e);

            nextY += 200;

            //////////////
            // Second View
            //////////////

            hdlView2.PrintRect = new RectangleF((printable.Left + printable.Width / 2), nextY + 2 * (marginFromBorder + verticalOffset), printable.Width / 2 - 20, 400);

            // Draws the second view
            DrawViewFrame(e.Graphics, "Second View", hdlView2.PrintRect, marginFromBorder);

            hdlView2.Print(e);
        }

        private void PrintPageRaster(object sender, System.Drawing.Printing.PrintPageEventArgs e,
                                     ref float nextY, int verticalOffset, RectangleF printable, int marginFromBorder)
        {
            //////////////
            // First View
            //////////////

            RectangleF printRect1 = new RectangleF(printable.Left, nextY, printable.Width - 20, 200);

            // Draws the first view with a 4x resolution for better quality
            Bitmap bmp1 = model1.RenderToBitmap(new Size((int)printRect1.Width * 4, (int)printRect1.Height * 4));

            System.Drawing.Size scaledSize;
            ScaleImageSizeToPrintRect(printRect1, bmp1.Size, out scaledSize);

            DrawViewFrame(e.Graphics, "First View", printRect1, marginFromBorder);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(bmp1, (int)(printRect1.Left + (printRect1.Width - scaledSize.Width) / 2), (int)(printRect1.Top + (printRect1.Height - scaledSize.Height) / 2), scaledSize.Width, scaledSize.Height);

            nextY += 200;

            //////////////
            // Second View
            //////////////

            Camera oldCamera = model1.Viewports[0].Camera;
            model1.Viewports[0].Camera = secondCamera;

            // Draws the second view
            RectangleF printRect2 = new RectangleF((printable.Left + printable.Width / 2), nextY + 2 * (marginFromBorder + verticalOffset), printable.Width / 2 - 20, 400);

            // Set the second view viewport size
            Size oldsize = model1.Size;
            model1.Size = new Size(200, 250);

            // Draws the second view with a 4x resolution for better quality
            Bitmap bmp2 = model1.RenderToBitmap(new Size((int)printRect2.Width * 1, (int)printRect2.Height * 1));

            // restore previous view viewport size
            model1.Size = oldsize;

            ScaleImageSizeToPrintRect(printRect2, bmp2.Size, out scaledSize);

            DrawViewFrame(e.Graphics, "Second View", printRect2, marginFromBorder);

            e.Graphics.DrawImage(bmp2, (int)(printRect2.Left + (printRect2.Width - scaledSize.Width) / 2), (int)(printRect2.Top + (printRect2.Height - scaledSize.Height) / 2), scaledSize.Width, scaledSize.Height);

            // restore previous camera
            model1.Viewports[0].Camera = oldCamera;
            model1.Invalidate();
        }

        private void ScaleImageSizeToPrintRect(RectangleF printRect, System.Drawing.Size imageSize, out System.Drawing.Size scaledSize)
        {
            double width, height;
            double ratio;

            // fit the width of the image inside the width of the print Rectangle
            ratio = printRect.Width / imageSize.Width;
            width = imageSize.Width * ratio;
            height = imageSize.Height * ratio;

            // fit the other dimension
            if (height > printRect.Height)
            {
                ratio = printRect.Height / height;
                width *= ratio;
                height *= ratio;
            }

            scaledSize = new System.Drawing.Size((int)width, (int)height);
        }

        private void DrawViewFrame(System.Drawing.Graphics graphics, string title, RectangleF printRect, int marginFromBorder)
        {
            Rectangle borderRectangle = new Rectangle((int)(printRect.X - marginFromBorder),
                                                        (int)(printRect.Y - marginFromBorder),
                                                        (int)(printRect.Width + 2 * marginFromBorder),
                                                        (int)(printRect.Height + 2 * marginFromBorder));

            // Draws the view title
            System.Drawing.SizeF titleSize = graphics.MeasureString(title, Font);
            graphics.DrawString(title, Font, System.Drawing.Brushes.Black, borderRectangle.Left + borderRectangle.Width / 2 - titleSize.Width / 2, borderRectangle.Top - titleSize.Height);

            // Draws a shadow rectangle
            graphics.FillRectangle(System.Drawing.Brushes.LightGray, borderRectangle.Left + 10, borderRectangle.Top + 10, borderRectangle.Width, borderRectangle.Height);
            graphics.FillRectangle(System.Drawing.Brushes.White, borderRectangle);

            // Draws the border of the viewport
            Pen borderPen = new Pen(System.Drawing.Color.Gray, 1);
            graphics.DrawRectangle(borderPen, borderRectangle);

        }

        private void comboBoxPrintMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem ci = (ComboBoxItem)comboBoxPrintMode.SelectedItem;
            string value = ci.Content.ToString();
            exportToEMFButton.IsEnabled = value.Equals("Vector", StringComparison.InvariantCultureIgnoreCase);
        }

        class MyHiddenLinesViewPrint : HiddenLinesViewOnPaper
        {
            public MyHiddenLinesViewPrint(HiddenLinesViewSettings data)
                : base(data)
            {

            }

            protected override void  WorkCompleted(Environment model)
            {
                // Avoid the automatic printing
            }
        }       

    }
}