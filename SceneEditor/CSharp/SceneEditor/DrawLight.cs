using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Labels;
using devDept.Geometry;
using devDept.Graphics;
using System;
using System.Drawing;

namespace WindowsApplication1
{
    class DrawLight
    {
        private int Type = -1;                          // 0: point, 1:spot, 2: directional, 3: directional Stationary
        private int NumLight = -1;                      // number of light assigned
        public LightSettings Light;                     // Light Settings of assigned light
        private Model ModelOriginal;  // model of the final view
        private Point3D CenterScene;                    // center of the world scene in ModelOriginal
        private double Radius;                          // distance radius choosen for drawing directional light(starting from the CenterScene)
        private Entity DrawnLight;                      // current Drawn light in the scene editor
        private LeaderAndText LightName;                // Label shown of the drawn light
        private string LayerName;                       // The name of the Layer used for drawing the light
        

        public DrawLight(Model model, int numLight, Point3D centerScene, double radius, string layerName)
        {
            ModelOriginal = model;
            NumLight = numLight;
            CenterScene = centerScene;
            Radius = radius;
            LayerName = layerName;

            switch (NumLight)
            {
                case 1:
                    Light = ModelOriginal.Light1;
                    break;
                case 2:
                    Light = ModelOriginal.Light2;
                    break;
                case 3:
                    Light = ModelOriginal.Light3;
                    break;
                case 4:
                    Light = ModelOriginal.Light4;
                    break;
                case 5:
                    Light = ModelOriginal.Light5;
                    break;
                case 6:
                    Light = ModelOriginal.Light6;
                    break;
                case 7:
                    Light = ModelOriginal.Light7;
                    break;
                case 8:
                    Light = ModelOriginal.Light8;
                    break;
            }
        }

        public void SetLight(int type, bool? active, double x, double y, double z, double dx, double dy, double dz, double spotExponent, double linearAttenuation, double spotAngle, bool? yieldShadow)
        {
            if (active != null) Light.Active = (bool) active;
            else Light.Active = false;

            Type = type;
            Light.Stationary = false;

            // sets the Spot Exponent value (used only in spot light)
            Light.SpotExponent = (spotExponent < 128) ? spotExponent : 128;

            // sets the Linear Attenuation value (used only in spot light)
            Light.LinearAttenuation = linearAttenuation;

            // sets the Angle value (used only in spot light)
            Light.SpotHalfAngle = Utility.DegToRad(spotAngle);

            // sets if YieldShadow is active (only one light at time)
            if (yieldShadow != null) Light.YieldShadow = (bool)yieldShadow;
            else Light.YieldShadow = false;

            // sets the start Position of the light (used only in non-directional light)
            Light.Position = new Point3D(x, y, z);

            // sets the direction of the light (used only in spot and directional light)
            if (new Point3D(dx, dy, dz) != Point3D.Origin)
            {
                Light.Direction = new Vector3D(dx, dy, dz);
                Light.Direction.Normalize();
            }

            if (Light.Active)
            {
                switch (Type)
                {
                    case 0:
                        Light.Active = true;
                        Light.Type = lightType.Point;
                        Light.Stationary = false;
                        break;
                    case 1:
                        Light.Active = true;
                        Light.Type = lightType.Spot;
                        Light.Stationary = false;
                        break;
                    case 2:
                        Light.Active = true;
                        Light.Type = lightType.Directional;
                        Light.Stationary = false;
                        break;
                    case 3:
                        Light.Active = true;
                        Light.Type = lightType.Directional;
                        Light.Stationary = true;
                        break;

                }
            }
        }

        public void Draw(Model model)
        {
            switch (Type)
            {
                case 0: // point
                    DrawPoint(out DrawnLight, out LightName);
                    break;
                case 1: // spot
                    DrawSpot(out DrawnLight, out LightName);
                    break;
                case 2: // Directional
                case 3: // Directional Stationary
                    DrawDirectional(out DrawnLight, out LightName);
                    break;
            }

            if (Light.Active)
                DrawnLight.Color = Color.FromArgb(220, Color.Yellow);
            else
              DrawnLight.Color = Color.FromArgb(100, Color.Gray);

            model.Entities.Add(DrawnLight, LayerName);
            model.Labels.Add(LightName);

            MoveIfStationary(model);
        }

        private void DrawPoint(out Entity drawnLight, out LeaderAndText lightName)
        {
            // draws point light like a Joint
            drawnLight = new Joint(Light.Position.X, Light.Position.Y, Light.Position.Z, 1, 1);
            drawnLight.ColorMethod = colorMethodType.byEntity;

            // draws name label
            lightName = new LeaderAndText(Light.Position + new Vector3D(0, 0, 1), "Light " + NumLight,
                                          new Font("Tahoma", 8.25f), Color.White, new Vector2D(0, 15));
        }

        private void DrawSpot(out Entity drawnLight, out LeaderAndText lightName)
        {
            double distance, kl, kc, kq;

            kl = Light.LinearAttenuation;
            kc = Light.ConstantAttenuation;
            kq = Light.QuadraticAttenuation;

            // sets distance considering attenuation values of the light
            if (kq.CompareTo(0.0) != 0)
                distance = (-kl + Math.Sqrt(kl * kl - 4 * kq * kc)) / (2 * kq);
            else
                distance = ((1 / 0.6) - kc) / kl;

            // draws spot light like a cone
            drawnLight = Mesh.CreateCone(Math.Tan(Light.SpotHalfAngle) * distance, 0, distance, 10);
            drawnLight.ColorMethod = colorMethodType.byEntity;

            // Aligns the direction of spot to the light direction
            Transformation t = new Align3D(Plane.XY, new Plane(Light.Direction * -1));
            drawnLight.Translate(0, 0, -distance);
            drawnLight.TransformBy(t);

            // translates the light spot to choosen position
            drawnLight.Translate(Light.Position.X, Light.Position.Y, Light.Position.Z);

            // draws name label
            lightName = new LeaderAndText(Light.Position, "Light " + NumLight,
                                          new Font("Tahoma", 8.25f), Color.White, new Vector2D(0, 15));
        }

        private void DrawDirectional(out Entity drawnLight, out LeaderAndText lightName)
        {
            // sets start position of the drawn light
            Point3D startPoint = (Point3D)CenterScene.Clone();
            startPoint.TransformBy(new Translation(Light.Direction * (-Radius)));

            // draws directional light like an arrow
            drawnLight = Mesh.CreateArrow(startPoint, Light.Direction, 0.2, 5, 0.6, 3, 10, Mesh.natureType.Smooth, Mesh.edgeStyleType.Free);
            drawnLight.ColorMethod = colorMethodType.byEntity;

            // draws name label
            lightName = new LeaderAndText(startPoint + new Vector3D(0, 0, 0.2), "Light " + NumLight,
                                          new Font("Tahoma", 8.25f), Color.White, new Vector2D(0, 15));
        }

        public void MoveIfStationary(Model model)
        {
            if (DrawnLight != null && Light.Stationary)
            {
                DeletePrevious(model);

                // gets world direction
                float[] direction, position;
                Light.GetLightDirection(ModelOriginal.Camera.ModelViewMatrix, out direction, out position);
                Vector3D newDirection = new Vector3D((double)direction[0], (double)direction[1], (double)direction[2]);
                newDirection.Negate();

                // gets start point of the new drawn light
                Point3D startNewPoint = (Point3D)CenterScene.Clone();
                startNewPoint.TransformBy(new Translation(newDirection * (-Radius)));

                // draws new direction like an arrow
                DrawnLight = Mesh.CreateArrow(startNewPoint, newDirection, 0.2, 5, 0.6, 3, 10, Mesh.natureType.Smooth, Mesh.edgeStyleType.Free);
                DrawnLight.ColorMethod = colorMethodType.byEntity;
                DrawnLight.Color = Color.FromArgb(220, Color.Yellow);

                // draws name label
                LightName = new LeaderAndText(startNewPoint, "Light " + NumLight,
                                              new Font("Tahoma", 8.25f), Color.White, new Vector2D(0, 15));

                model.Entities.Add(DrawnLight, LayerName);
                model.Labels.Add(LightName);
            }
        }

        public void DeletePrevious(Model model)
        {
            if (DrawnLight != null)
            {
                // deletes previous light
                int index = model.Entities.IndexOf(DrawnLight);
                model.Entities[index].Selected = true;
                model.Entities.DeleteSelected();

                // deletes previous label
                int indexL = model.Labels.IndexOf(LightName);
                model.Labels[indexL].Selected = true;
                model.Labels.DeleteSelected();
            }
        }
    }
}
