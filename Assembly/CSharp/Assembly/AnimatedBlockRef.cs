using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace WpfApplication1
{
    class AnimatedBlockRef : BlockReference
    {
        public Quaternion startOrientation, finalOrientation;
        public bool animate;
        public double deltaT;
        private double t = 0;

        public Vector3D translationVect;
        private Vector3D rotationAxis;
        double rotationAngle;
        public Point3D firstPt;

        public AnimatedBlockRef(Transformation t, string blockName)
            : base(t, blockName)
        {
        }

        public void Init()
        {
            Vector3D axis;
            double angle;

            t = 0;

            startOrientation.ToAxisAngle(out axis, out angle);
            axis.Negate();
            Quaternion inverseQuat = new Quaternion(axis, angle);

            Quaternion q = finalOrientation * inverseQuat;
            q.ToAxisAngle(out rotationAxis, out rotationAngle);
        }

        protected override void Animate(int frameNumber)
        {
            if (!animate)
                return;

            t += deltaT;

            if (t > 1) // Stops the animation
            {
                animate = false;
                t = 1;
            }
        }

        public override void MoveTo(DrawParams data)
        {

            if (t != 0 && rotationAxis != null)
            {
                data.RenderContext.TranslateMatrixModelView(t * translationVect.X, t * translationVect.Y, t * translationVect.Z);
                base.MoveTo(data);
                data.RenderContext.TranslateMatrixModelView(firstPt.X, firstPt.Y, firstPt.Z);
                data.RenderContext.RotateMatrixModelView(t * rotationAngle, rotationAxis.X, rotationAxis.Y, rotationAxis.Z);
                data.RenderContext.TranslateMatrixModelView(-firstPt.X, -firstPt.Y, -firstPt.Z);

            }
            else
                base.MoveTo(data);
        }

        public override bool IsInFrustum(FrustumParams data, Point3D center, double radius)
        {
            Point3D transfCenter = (Point3D)center.Clone();

            if (t != 0 && rotationAxis != null)
            {
                // Get the BlockReference full transformation
                Transformation brFullTransf = GetFullTransformation(data.Blocks);

                // Apply the inverse of the full transformation to the center to bring it back to original position
                // It's necessary because in the MoveTo() the first transformation is applied before tha base method is called.
                Transformation tr = (Transformation)brFullTransf.Clone();
                tr.Invert();
                transfCenter.TransformBy(tr);

                // Compute a transformation equals to the transformation applied in the MoveTo method
                Translation translation1 = new Translation(t * translationVect.X, t * translationVect.Y, t * translationVect.Z);
                Translation translation2 = new Translation(firstPt.X, firstPt.Y, firstPt.Z);
                Translation translation3 = new Translation(-firstPt.X, -firstPt.Y, -firstPt.Z);
                Rotation rotation = new Rotation(Utility.DegToRad(t * rotationAngle), rotationAxis);

                Transformation customTransform = translation1 * brFullTransf;
                customTransform = customTransform * translation2;
                customTransform = customTransform * rotation;
                customTransform = customTransform * translation3;

                // Apply transformation to the center
                transfCenter.TransformBy(customTransform);
            }

            // Call the base with the transformed "center", to avoid undesired clipping
            return base.IsInFrustum(data, transfCenter, radius);
        }
    }
}