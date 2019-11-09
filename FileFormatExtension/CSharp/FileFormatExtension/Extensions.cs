using devDept.Eyeshot.Entities;
using devDept.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using devDept.Eyeshot;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using ProtoBuf.Meta;

// Here we use a specific namespace to ensure the compatibility between C# and VB.NET samples for the custom object named "CustomData".
namespace EyeshotExtensions
{

    #region MyCircle

    /// <summary>
    /// Defines an extension for the Circle entity.
    /// </summary>    
    public class MyCircle : Circle
    {
        #region Constructors

        public MyCircle(Plane plane, double radius) : base(plane, radius)
        {
        }

        public MyCircle(Circle another) : base(another)
        {
        }
        
        #endregion

        public string CustomDescription { get; set; }

        public override string Dump()
        {
            StringBuilder sb = new StringBuilder(base.Dump());

            sb.AppendLine("CustomDescription = " + CustomDescription);

            CustomData cd = EntityData as CustomData;

            if (cd != null)
            {
                sb.AppendLine("----------------------");
                sb.AppendLine("CustomData");                
                sb.Append(cd.Dump());                
            }

            return sb.ToString();
        }

        #region Proprietary file format
        public override EntitySurrogate ConvertToSurrogate()
        {
            return new MyCircleSurrogate(this);
        }

        #endregion
    }

    /// <summary>
    /// Defines the <see cref="MyCircle"/> surrogate.
    /// </summary>
    public class MyCircleSurrogate : CircleSurrogate
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>            
        public MyCircleSurrogate(MyCircle myCircle) : base(myCircle)
        {

        }
        
        /// <summary>
        /// My custom description.
        /// </summary>
        public string CustomDescription { get; set; }

        /// <summary>
        /// Creates the object related to the surrogate.
        /// </summary>        
        ///<remarks>
        /// This method uses the <see cref="MyCircle.MyCircle(Plane, double)"/> constructor to create the object.
        /// When the content is <see cref="contentType.Tessellation"/> the resulting object is a <see cref="LinearPath"/>
        /// </remarks>
        /// <returns>The object created.</returns>
        protected override Entity ConvertToObject()
        {
            Entity ent;
            if (DeserializationContent == contentType.Tessellation)
            {
                // When the content is "Tessellation only" we create a LinearPath instead of a MyCircle.
                // If the entity was stored without vertices data, we add a Ghost entity as placeholder.

                if (CheckSurrogateData(DeserializationContent, String.Empty)) // pass string empty so no log is written                
                {
                    ent = new LinearPath(Vertices);
                }
                else
                {
                    ent = new Ghost("MyCircle without tessellation data.");
                    WriteLog("MyCircle without tessellation data has been created as Ghost entity.");
                }
            }
            else
                ent = new MyCircle(Plane, Radius);

            CopyDataToObject(ent);

            return ent;
        }

        /// <summary>
        /// Copies common data from surrogate to object.
        /// </summary>
        /// <remarks>This method is called by the ConvertToObject method after the creation of the object instance.</remarks>
        protected override void CopyDataToObject(Entity entity)
        {
            var myCircle = entity as MyCircle;
            if (myCircle != null)
                myCircle.CustomDescription = CustomDescription;

            base.CopyDataToObject(entity);
        }

        /// <summary>
        /// Copies all data from the object to its surrogate.
        /// </summary>
        /// <remarks>Use this method to fill ALL the properties of this surrogate. It is called by the empty constructor to initialize the surrogates properties.</remarks>        
        protected override void CopyDataFromObject(Entity entity)
        {
            var myCircle = entity as MyCircle;
            if (myCircle != null)
                CustomDescription = myCircle.CustomDescription;

            base.CopyDataFromObject(entity);
        }

        /// <summary>
        /// Integrity check according to the content type.
        /// </summary>                
        /// <remarks>        
        /// During the serialization process, this method is called internally before serializing the surrogate.        
        /// During the deserialization process, it can be used in the ConvertToObject method.
        ///  </remarks>
        protected override bool CheckSurrogateData(contentType content, string logMessage = null)
        {
            if (content == contentType.Tessellation)
            {
                if (Vertices == null || Vertices.Length == 0)
                {
                    WriteLog(logMessage != null ? logMessage : "Warning MyCircle with no vertices.");
                    return false;
                }
            }

            return true;
        }
    }

    #endregion


    #region Custom data

    /// <summary>
    /// Defines a custom object.
    /// </summary>
    /// <remarks>Compile the project with the conditional symbol "OLDVER" to simulate an old version of this class.</remarks>
    public class CustomData
    {
        #region Constructors

        /// <summary>
        /// Constructor for custom version 1.1
        /// </summary>
        /// <seealso cref="MyFileSerializer.CustomTag"/>
        public CustomData(int id)
        {
            Id = id;        
        }

#if !OLDVER
        /// <summary>
        /// Constructor for custom version 1.2
        /// </summary>
        /// <seealso cref="MyFileSerializer.CustomTag"/>
        public CustomData(int id, float price)
        {
            Id = id;
            Price = price;
        }
#endif

        #endregion

        #region Properties

        public int Id { get; set; }        

        public string Description { get; set; }

#if !OLDVER        
        public float Price { get; set; }
#endif

        #endregion

        public virtual string Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Id = " + Id);
            sb.AppendLine("Description = " + Description);
#if !OLDVER        
            sb.AppendLine("Price = " + Price);
#endif
            
            return sb.ToString();
        }

        #region Proprietary file format
        public virtual CustomDataSurrogate ConvertToSurrogate()
        {
            return new CustomDataSurrogate(this);
        }
        #endregion
    }

    /// <summary>
    /// Defines the <see cref="CustomData"/> surrogate.
    /// </summary>    
    public class CustomDataSurrogate : Surrogate<CustomData>
    {
        #region Constructors                        

        public CustomDataSurrogate(CustomData obj)
            : base(obj) // The base calls the CopyDataFromObject method.
        {
        }

        #endregion

        #region Properties            

        public int Id { get; set; }

        public string Description { get; set; }

        public float Price { get; set; }

        #endregion

        #region Methods                

        protected override CustomData ConvertToObject()
        {

            CustomData cd = null;

#if !OLDVER
            // Here you can use the Tag to handle different behavior for different versions.
            if (Tag == "1.1")
                cd = new CustomData(Id);
            else if (Tag == "1.2")
                cd = new CustomData(Id, Price);
#else
            cd = new CustomData(Id);
#endif

            CopyDataToObject(cd);
            return cd;
        }

        protected override void CopyDataToObject(CustomData cd)
        {
            cd.Description = Description;
#if !OLDVER
            if (Tag == "1.1")
                cd.Price = 100; // I want to force the price for object stored with the old version.            
#endif
        }

        protected override void CopyDataFromObject(CustomData cd)
        {
            Id = cd.Id;
            Description = cd.Description;
#if !OLDVER
            Price = cd.Price;
#endif
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Converts the surrogate to the related object during the deserialization process.
        /// </summary>        
        public static implicit operator CustomData(CustomDataSurrogate surrogate)
        {
            return surrogate == null ? null : surrogate.ConvertToObject();
        }

        /// <summary>
        /// Converts the object to the related surrogate during the serialization process.
        /// </summary>
        public static implicit operator CustomDataSurrogate(CustomData source)
        {
            return source == null ? null : source.ConvertToSurrogate();
        }

        #endregion
    }

    #endregion


    /// <summary>
    /// Defines an extension for the Eyeshot proprietary file format.
    /// </summary>
    /// <remarks>
    /// If you want to include special Autodesk objects like LayerEx, CircleEx, PictureEx, etc. you need to derive from FileSerializerEx contained in the x86/x64 Eyeshot assemblies.
    /// </remarks>
    public class MyFileSerializer : FileSerializer
    {
        // Tag used to handle different versions of your custom objects.
        public static string CustomTag
#if OLDVER
            = "1.1";
#else
            = "1.2";
#endif

        #region Constructors

        /// <summary>
        /// Empty constructor used in conjunction with the <see cref="WriteFile"/> class that accepts the <see cref="Model"/> as parameter.
        /// </summary>        
        /// <remarks>Use this constructor to define the serialization model for the <see cref="WriteFile"/> class that accepts the <see cref="Model"/> as parameter.</remarks>        
        public MyFileSerializer()
        {
        }

        /// <summary>
        /// Constructor used in conjunction with the <see cref="ReadFile"/> class.
        /// </summary>        
        /// <exception cref="EyeshotException">Thrown if the content type is <see cref="contentType.None"/>.</exception>
        /// <remarks>Use this constructor to define the serialization model for the <see cref="ReadFile"/> class.        
        public MyFileSerializer(contentType contentType) : base(contentType)
        {
        }

        #endregion

        protected override void FillModel()
        {
            base.FillModel();            

            // Adds MyCircle as sub-type of Circle
            // When you add a sub-type to an Eyeshot object you have to use an id > 1000.
            Model[typeof(Circle)]
                .AddSubType(1001, typeof(MyCircle));

            // Adds MyCircleSurrogate as sub-type of CircleSurrogate
            Model[typeof(CircleSurrogate)]
                .AddSubType(1001, typeof(MyCircleSurrogate));

            // Defines properties for MyCircleSurrogate
            Model[typeof(MyCircleSurrogate)]
                .Add(1, "CustomDescription")
                .UseConstructor = false;

            // Adds the CustomData to the protobuf model and defines its surrogate.
            Model.Add(typeof(CustomData), false)
                .SetSurrogate(typeof(CustomDataSurrogate));

            // Defines properties for CustomDataSurrogate
            MetaType mt = Model[typeof(CustomDataSurrogate)]
                .Add(1, "Id")                
                .Add(2, "Description");

            // Use the header tag to handle different definitions for your custom model. 
            if (this.HeaderTag == "1.2")
            {                
                // Never use the same id for different properties of the same object.
                mt.Add(3, "Price");
            }

            mt.SetCallbacks(null, null, "BeforeDeserialize", null); // Fills Version and Tag during the deserialization.
            mt.UseConstructor = false; // Avoids to use the parameterless constructor.
        }
    }
}
