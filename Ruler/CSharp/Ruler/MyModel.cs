using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Geometry;
using System.Drawing;

namespace WpfApplication1
{
    public class MyModel : devDept.Eyeshot.Model
    {
        Point2D lowerLeftWorldCoord, upperRightWorldCoord;
        Point2D originScreen;

        public enum rulerPlaneType
        {
            ZX, YZ, XY
        }

        public rulerPlaneType RulerPlaneMode;
        Plane rulerPlane;

        protected override void DrawViewport(DrawSceneParams myParams)
        {
            base.DrawViewport(myParams);

            switch (RulerPlaneMode)
            {
                case rulerPlaneType.XY:
                    rulerPlane = Plane.XY;
                    break;
                case rulerPlaneType.ZX:
                    rulerPlane = Plane.ZX;
                    break;
                case rulerPlaneType.YZ:
                    rulerPlane = Plane.YZ;
                    break;
            }

            // Get the world coordinates of the corners and the screen position of the origin for reference

            Point3D ptUpperRight, ptLowerLeft;            
            ScreenToPlane(new System.Drawing.Point((int)Size.Width, rulerSize), rulerPlane, out ptUpperRight);
            ScreenToPlane(new System.Drawing.Point(rulerSize, (int)Size.Height), rulerPlane, out ptLowerLeft);

            originScreen = WorldToScreen(0, 0, 0);

            switch (RulerPlaneMode)
            {
                case rulerPlaneType.XY:
                    upperRightWorldCoord = new Point2D(ptUpperRight.X, ptUpperRight.Y);
                    lowerLeftWorldCoord = new Point2D(ptLowerLeft.X, ptLowerLeft.Y);
                    break;
                case rulerPlaneType.ZX:
                    upperRightWorldCoord = new Point2D(ptUpperRight.X, ptUpperRight.Z);
                    lowerLeftWorldCoord = new Point2D(ptLowerLeft.X, ptLowerLeft.Z);
                    break;
                case rulerPlaneType.YZ:
                    upperRightWorldCoord = new Point2D(ptUpperRight.Y, ptUpperRight.Z);
                    lowerLeftWorldCoord = new Point2D(ptLowerLeft.Y, ptLowerLeft.Z);
                    break;
            }

        }

        int rulerSize = 35;

        protected override void DrawOverlay(DrawSceneParams myParams)
        {
            double Height = Size.Height;
            double Width = Size.Width;

            renderContext.SetState(depthStencilStateType.DepthTestOff);
            renderContext.SetState(blendStateType.Blend);

            // Draw the transparent ruler
            renderContext.SetColorWireframe(Color.FromArgb((int)(0.4 * 255), 255, 255, 255));
            renderContext.SetState(rasterizerStateType.CCW_PolygonFill_NoCullFace_NoPolygonOffset);

            // Vertical Ruler
            renderContext.DrawQuad(new RectangleF(0, 0, rulerSize, (float) (Height - rulerSize)));

            // Horizontal Ruler
            renderContext.DrawQuad(new RectangleF(rulerSize, (float)(Height - rulerSize), (float)(Width - rulerSize), rulerSize));                       

            renderContext.SetState(blendStateType.NoBlend);

            // choose a string format with 2 decimal numbers
            string formatString = "{0:0.##}";

            double worldToScreen = (Height - rulerSize) / (upperRightWorldCoord.Y - lowerLeftWorldCoord.Y);

            double stepWorldX = 5, stepWorldY = 5;

            double worldHeight = upperRightWorldCoord.Y - lowerLeftWorldCoord.Y;
            double nlinesH = (worldHeight / stepWorldY);

            double worldWidth = upperRightWorldCoord.X - lowerLeftWorldCoord.X;
            double nlinesW = (worldWidth / stepWorldX);

            RefineStep(nlinesH, worldHeight, ref stepWorldY);
            RefineStep(nlinesW, worldWidth, ref stepWorldX);

            double stepWorld = Math.Min(stepWorldX, stepWorldY);

            double stepScreen = stepWorld * worldToScreen;

            ///////////////////////////
            // Vertical ruler
            ///////////////////////////

            // First line Y world coordinate
            double startYWorld = stepWorld * Math.Floor(lowerLeftWorldCoord.Y / stepWorld);

            Point2D firstLineScreenPositionY = new Point2D(0, originScreen.Y + startYWorld * worldToScreen);
            double currentScreenY = firstLineScreenPositionY.Y;
            double shorterLineXPos = (firstLineScreenPositionY.X + rulerSize) / 2;

            // draw a longer line each 5 lines. The origin must be a longer line.
            int countShortLinesY = (int)(Math.Round((currentScreenY - originScreen.Y) / stepScreen)) % 5;

            // Draw the ruler lines
            renderContext.SetLineSize(1);

            double left;

            Font myFont = UtilityEx.GetFont(FontFamily, FontStyle, FontWeight, FontSize);

            for (double y = startYWorld; y < upperRightWorldCoord.Y; y += stepWorld, currentScreenY += stepScreen)
            {
                if (countShortLinesY % 5 == 0)
                    left = firstLineScreenPositionY.X;
                else
                    left = shorterLineXPos; ;

                renderContext.SetColorWireframe(Color.Black);

                renderContext.DrawLine(new Point2D(left, currentScreenY), new Point2D(rulerSize, currentScreenY));

                DrawText((int)firstLineScreenPositionY.X, (int)currentScreenY, string.Format(formatString, y), myFont, Color.Black, ContentAlignment.BottomLeft);

                countShortLinesY++;
            }


            ///////////////////////////
            // Horizontal ruler
            ///////////////////////////

            // First line X world coordinate
            double startXWorld = stepWorld * Math.Ceiling(lowerLeftWorldCoord.X / stepWorld);

            Point2D firstLineScreenPositionX = new Point2D(originScreen.X + startXWorld * worldToScreen, 0);
            double currentScreenX = firstLineScreenPositionX.X;
            double shorterLineYPos = (firstLineScreenPositionX.Y + rulerSize) / 2;

            int countShortLinesX = (int)(Math.Round((currentScreenX - originScreen.X) / stepScreen)) % 5;

            double top;
            for (double x = startXWorld; x < upperRightWorldCoord.X; x += stepWorld, currentScreenX += stepScreen)
            {
                if (countShortLinesX % 5 == 0)
                    top = firstLineScreenPositionX.Y;
                else
                    top = shorterLineYPos;

                renderContext.SetColorWireframe(Color.Black);

                renderContext.DrawLine(new Point2D(currentScreenX, Height - top), new Point2D(currentScreenX, Height - rulerSize));

                DrawText((int)currentScreenX, (int)(Height - rulerSize - firstLineScreenPositionX.Y), string.Format(formatString, x), myFont, Color.Black, ContentAlignment.BottomLeft);

                countShortLinesX++;
            }

            // Draw a red line in correspondence with the mouse position
            
            renderContext.SetColorWireframe(Color.Red);
            
            if (mousePos.Y > rulerSize)
            {
                renderContext.DrawLine(new Point3D(0, Height - mousePos.Y, 0), new Point3D(rulerSize, Height - mousePos.Y, 0));
            }

            if (mousePos.X > rulerSize)
            {
                renderContext.DrawLine(new Point3D(mousePos.X, Height, 0), new Point3D(mousePos.X, Height - rulerSize, 0));
            }

            // Draw the logo image at the bottom right corner            
            Bitmap logo = new Bitmap("../../../../../../dataset/Assets/Pictures/Logo.png");
            DrawImage(Size.Width - logo.Width - 20, 20, logo);

            // call the base function
            base.DrawOverlay(myParams);
        }

        private static void RefineStep(double nlines, double worldLength, ref double stepWorld)
        {
            // Refine the step if there are too many ruler lines, or too few
            if (nlines > 20)
            {
                do
                {
                    stepWorld *= 2;
                    nlines = (worldLength/stepWorld);
                } while (nlines > 20);
            }
            else if (nlines < 10)
            {
                do
                {
                    stepWorld /= 2;
                    nlines = (worldLength/stepWorld);
                } while (nlines < 10 && nlines > 0);
            }            
        }

        private System.Drawing.Point mousePos;

        protected override void OnMouseMove(MouseEventArgs e)
        {            
            base.OnMouseMove(e);

            mousePos = RenderContextUtility.ConvertPoint(GetMousePosition(e));

            PaintBackBuffer();
            SwapBuffers();
        }
    }

}