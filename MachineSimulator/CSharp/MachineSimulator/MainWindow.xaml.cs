using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private const string Assets = "../../../../../../dataset/Assets/";

        private int _interval = 20; // medium speed
        private int _animationFrameNumber = -1;

        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.            
        }

        protected override void OnContentRendered(EventArgs e)
        {
            model1.GetGrid().AutoSize = true;
            model1.GetGrid().Step = 500;

            devDept.Eyeshot.Translators.ReadFile readFile = new devDept.Eyeshot.Translators.ReadFile(Assets + "160.eye");
            readFile.DoWork();
            model1.Entities.AddRange(readFile.Entities, System.Drawing.Color.DimGray);

            Block firstBlock = new Block("First");
            AddStlToBlock(firstBlock, "930.eye", System.Drawing.Color.DeepSkyBlue);
            AddStlToBlock(firstBlock, "940.eye", System.Drawing.Color.DeepSkyBlue);

            Block secondBlock = new Block("Second");
            AddStlToBlock(secondBlock, "570.eye", System.Drawing.Color.DodgerBlue);

            Block thirdBlock = new Block("Third");
            AddStlToBlock(thirdBlock, "590.eye", System.Drawing.Color.SlateBlue);

            firstBlock.Entities.Add(new TranslatingAlongY("Second"));
            secondBlock.Entities.Add(new TranslatingAlongZ("Third"));

            model1.Blocks.Add(firstBlock);
            model1.Blocks.Add(secondBlock);
            model1.Blocks.Add(thirdBlock);

            model1.Entities.Add(new TranslatingAlongX("First"));

            model1.SetView(viewType.Trimetric);

            model1.ZoomFit();           

            // Turn off silhouettes to increase drawing speed
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;

            // Shadows are not currently supported in animations
            model1.Rendered.ShadowMode = shadowType.None;

            model1.StartAnimation(_interval);
            startButton.IsEnabled = false;

            base.OnContentRendered(e);
        }

        private void AddStlToBlock(Block block, string eyeName, System.Drawing.Color color)
        {
            devDept.Eyeshot.Translators.ReadFile readFile = new devDept.Eyeshot.Translators.ReadFile(Assets + eyeName);

            readFile.DoWork();


            readFile.Entities[0].ColorMethod = colorMethodType.byEntity;

            readFile.Entities[0].Color = color;

            block.Entities.Add(readFile.Entities[0]);
        }

        private void StartButtonEnable(bool value)
        {
            startButton.IsEnabled = value;
            pauseButton.IsEnabled = !value;
            stopButton.IsEnabled = !value;
        }

        private bool IsAnimationStarted()
        {
            return !startButton.IsEnabled;
        }

        // Starts the animation
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (_animationFrameNumber != -1)
                model1.AnimationFrameNumber = _animationFrameNumber;
            model1.StartAnimation(_interval);
            StartButtonEnable(false);
        }

        // After this method the animation starts from where it was stopped, saving animationFrameNumber
        private void PauseButton_Click(object sender, EventArgs e)
        {
            _animationFrameNumber = model1.AnimationFrameNumber;
            model1.StopAnimation();
            StartButtonEnable(true);
        }

        // After this method the animation starts from the beginning
        private void StopButton_Click(object sender, EventArgs e)
        {
            model1.StopAnimation();
            StartButtonEnable(true);
        }

        // Changes the speed of the animation by the interval. initially interval=20 (medium)
        private void ChangeSpeedAnimation(int interval)
        {
            _interval = interval;
            _animationFrameNumber = model1.AnimationFrameNumber;
            if (IsAnimationStarted())
            {
                model1.StopAnimation();
                if (_animationFrameNumber != -1)
                    model1.AnimationFrameNumber = _animationFrameNumber;
                model1.StartAnimation(_interval);
            }
        }

        private void SlowRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ChangeSpeedAnimation(99);
        }

        private void MediumRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ChangeSpeedAnimation(20);
        }

        private void FastRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ChangeSpeedAnimation(1);
        }
    }

    class TranslatingAlongX : BlockReference
    {

        private double xPos;
        private Translation customTransform;

        public TranslatingAlongX(string blockName)
            : base(0, 0, 0, blockName, 1, 1, 1, 0)
        {
        }

        protected override void Animate(int frameNumber)
        {
            // frameNumber is incremented each time this function is called
            // it represents the time passing an can be used to index an array
            // 3D positions for example.

            // angle in degrees
            double alpha = (frameNumber % 359) * 10;

            // circle radius
            double radius = 100;

            xPos = radius * Math.Cos(Utility.DegToRad(alpha));

            base.Animate(frameNumber);
        }

        public override void MoveTo(DrawParams data)
        {
            base.MoveTo(data);

            customTransform = new Translation(xPos, 0, 0);
            data.RenderContext.MultMatrixModelView(customTransform);
        }

        public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
        {
            // Call the base with the transformed "center", to avoid undesired clipping
            return base.IsInFrustum(data, customTransform * center, radius);
        }

    }

    class TranslatingAlongY : BlockReference
    {
        private double xPos, yPos;
        private Translation customTransform;

        public TranslatingAlongY(string blockName)
            : base(0, 0, 0, blockName, 1, 1, 1, 0)
        {
        }

        protected override void Animate(int frameNumber)
        {
            // frameNumber is incremented each time this function is called
            // it represents the time passing an can be used to index an array
            // 3D positions for example.

            // angle in degrees
            double alpha = (frameNumber % 359) * 10;

            // circle radius
            double radius = 100;

            yPos = radius * Math.Sin(Utility.DegToRad(alpha));

            base.Animate(frameNumber);
        }

        public override void MoveTo(DrawParams data)
        {
            base.MoveTo(data);

            customTransform = new Translation(xPos, yPos, 0);
            data.RenderContext.MultMatrixModelView(customTransform);
        }

        public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
        {
            // Call the base with the transformed "center", to avoid undesired clipping
            return base.IsInFrustum(data, customTransform * center, radius);
        }

    }

    class TranslatingAlongZ : BlockReference
    {

        private double xPos, yPos, zPos;
        private Transformation customTransform;

        public TranslatingAlongZ(string blockName)
            : base(0, 0, 0, blockName, 1, 1, 1, 0)
        {
        }

        protected override void Animate(int frameNumber)
        {
            // frameNumber is incremented each time this function is called
            // it represents the time passing an can be used to index an array
            // 3D positions for example.

            // angle in degrees
            double alpha = (frameNumber % 359) * 10;

            // circle radius
            double radius = 100;

            zPos = radius * Math.Cos(Utility.DegToRad(alpha));

            base.Animate(frameNumber);
        }

        public override void MoveTo(DrawParams data)
        {
            base.MoveTo(data);

            customTransform = new Translation(xPos, yPos, zPos);
            data.RenderContext.MultMatrixModelView(customTransform);
        }

        public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
        {
            // Call the base with the transformed "center", to avoid undesired clipping
            return base.IsInFrustum(data, customTransform * center, radius);
        }

    }
}