using devDept.Eyeshot.Translators;
using devDept.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using devDept.Eyeshot;

namespace WpfApplication1
{
    /// <summary>
    /// Helper class for dynamic loading of the Eyeshot x86 or x64 assembly.
    /// </summary>
    class BitnessAgnostic
    {
        public BitnessAgnostic()
        {
            devDept.Eyeshot.Environment.GetAssembly(out Product, out Title, out Company, out Version, out Edition);

            string target = "x86";
            // Use the following code to check Operating System bitness
            // string target = System.Environment.Is64BitProcess ? "x64" : "x86";

            string platform = Title.ToLower().Contains("wpf") ? "Wpf" : "Win";

            AssemblyPath = String.Format(@"{0}\Bin\{1}\devDept.Eyeshot.Control.{1}.{2}.v{3}.dll", GetInstallFolderFromRegistry(), target, platform, Version.Major);
                        
            if (System.IO.File.Exists(AssemblyPath))
                Assembly = System.Reflection.Assembly.LoadFrom(AssemblyPath);
        }

        public readonly string AssemblyPath;
        public readonly Version Version;
        public readonly string Product, Title, Company, Edition;
        public readonly System.Reflection.Assembly Assembly;        

        /// <summary>
        /// Gets an instance of the ReadAutodesk class.
        /// </summary>        
        public ReadFileAsync GetReadAutodesk(Model model, string fileName)
        {
            if (Assembly == null)
                return null;

            var foundType = GetObjectType("ReadAutodesk");

            if (foundType != null)
            {
                // invokes the constructor                
                System.Reflection.ConstructorInfo constructor =
                    foundType.GetConstructor(new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool) });
                if (constructor != null)
                {
                    // parameters are: fileName, password, fixErrors, skipHatches
                    object reader = constructor.Invoke(new object[] { fileName, null, false, false, false });

                    return reader as ReadFileAsync;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an instance of the WriteAutodesk class.
        /// </summary>        
        public WriteFileAsync GetWriteAutodesk(Model model, string fileName)
        {
            if (Assembly == null)
                return null;

            var writeAutodeskParamsType = GetObjectType("WriteAutodeskParams");

            if (writeAutodeskParamsType != null)
            {
                // invokes the WriteAutodeskParams constructor                
                System.Reflection.ConstructorInfo constructor = writeAutodeskParamsType.GetConstructor(new Type[] { typeof(Model), typeof(Drawings), typeof(bool), typeof(bool), typeof(double) });
                if (constructor != null)
                {
                    // parameter is model
                    object writeAutodeskParams = constructor.Invoke(new object[] { model, null, false, false, 1 });

                    var writeAutodeskType = GetObjectType("WriteAutodesk");

                    if (writeAutodeskType != null)
                    {
                        // invokes the WriteAutodesk constructor                
                        constructor = writeAutodeskType.GetConstructor(new Type[] { writeAutodeskParamsType, typeof(string) });
                        if (constructor != null)
                        {
                            // parameters are: writeAutodeskParams, fileName
                            object writer = constructor.Invoke(new object[] { writeAutodeskParams, fileName });

                            return writer as WriteFileAsync;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an instance of the WritePDF class.
        /// </summary>        
        public WriteFileAsync GetWritePDF(Model model, string fileName)
        {
            if (Assembly == null)
                return null;

            // wfa = new WritePDF(new WritePdfParams(model1, new Size(595, 842), new Rectangle(10, 10, 575, 822), Color.White), saveFileDialog.FileName);

            var writePdfParamsType = GetObjectType("WritePdfParams");

            if (writePdfParamsType != null)
            {
                // invokes the WritePdfParams constructor                
                System.Reflection.ConstructorInfo constructor = writePdfParamsType.GetConstructor(new Type[] { typeof(Model), typeof(Size), typeof(Rect) });
                if (constructor != null)
                {
                    // parameter is Model, Size, Rectangle, Color
                    object writePdfParams = constructor.Invoke(new object[] { model, new Size(595, 842), new Rect(10, 10, 575, 822) });

                    var writePDFType = GetObjectType("WritePDF");

                    if (writePDFType != null)
                    {
                        // invokes the WritePDF constructor                
                        constructor = writePDFType.GetConstructor(new Type[] { writePdfParamsType, typeof(string) });
                        if (constructor != null)
                        {
                            // parameters are: WritePdfParams, fileName
                            object writer = constructor.Invoke(new object[] { writePdfParams, fileName });

                            return writer as WriteFileAsync;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an instance of the FileSerializerEx class.
        /// </summary>        
        public FileSerializer GetFileSerializerEx(contentType? contentType = null)
        {
            if (Assembly == null)
                return null;

            var foundType = GetObjectType("FileSerializerEx");

            if (foundType != null)
            {
                // invokes the constructor                
                System.Reflection.ConstructorInfo constructor = contentType == null ? foundType.GetConstructor(new Type[0]) : foundType.GetConstructor(new Type[] { typeof(contentType) });
                if (constructor != null)
                {
                    // parameters are: fileName, password, fixErrors, skipHatches
                    object serializer = contentType == null ? constructor.Invoke(new object[0]) : constructor.Invoke(new object[] { contentType });

                    return serializer as FileSerializer;
                }
            }

            return null;
        }

        #region Helper methods

        private string GetInstallFolderFromRegistry()
        {
            // Open a subKey as read-only
            using (Microsoft.Win32.RegistryKey sk1 = GetdevDeptRegistryKey(Microsoft.Win32.Registry.LocalMachine))
            {

                try
                {
                    return (string)sk1.GetValue("Install folder " + Version.Major);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private static Microsoft.Win32.RegistryKey GetdevDeptRegistryKey(Microsoft.Win32.RegistryKey baseKey)
        {
            try
            {
                if (System.Environment.Is64BitProcess)
                    return baseKey.OpenSubKey(@"Software\Wow6432Node\devDept Software\");

                return baseKey.OpenSubKey(@"Software\devDept Software\");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

        }

        private Type GetObjectType(string className)
        {
            Type foundType = null;
            Type[] types = Assembly.GetExportedTypes();

            if (types.Length > 0)
            {
                foreach (Type type in types)
                {
                    if (type.Name.Equals(className, StringComparison.OrdinalIgnoreCase))
                    {
                        foundType = type;
                        break;
                    }
                }
            }

            return foundType;
        }

        #endregion Helper methods
    }
}
