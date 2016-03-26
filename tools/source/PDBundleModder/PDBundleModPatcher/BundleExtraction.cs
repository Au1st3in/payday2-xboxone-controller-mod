using DieselBundle;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PDBundleModPatcher
{
    public class LogEntry
    {
        private String message;

        public LogEntry()
        {
            this.message = "";
        }

        public LogEntry(String message)
        {
            this.message = message;
        }

        public String getMessage()
        {
            return message;
        }

        public override string ToString()
        {
            return message.ToString();
        }
    }

    public class BundleExtraction
    {
        static private NameIndex NameIndex = StaticStorage.Index;
        static private KnownIndex KnownIndex = StaticStorage.Known_Index;
        static private BundleHeader bundle = new BundleHeader();
        public Queue<LogEntry> log = new Queue<LogEntry>();
        public uint progress {get; set;}
        public uint total_bundle {get; set;}
        public uint current_bundle {get; set;}
        public bool Finished { get; set; }
        public bool MainEnabled { get; set; }
        public bool extract_one {get;set;}
        public string extract_id {get; set;}
        public string listlog = "./listlog.log";
        public StreamWriter listlogger;

        public LogEntry[] getLog()
        {
            return log.ToArray();
        }

        public void TextWriteLine(string line, object line2 = null, object line3 = null)
        {
            log.Enqueue(new LogEntry(StaticStorage.log.writeLine(string.Format(line, line2, line3), true)));
            //this.ExtractLogBox.ScrollToCaret();
        }

        public void Start()
        {
            StartBundle();
            Finished = true;
        }

        public void StartBundle()
        {
            if (!String.IsNullOrWhiteSpace(StaticStorage.settings.ListLogFile))
            {
                listlog = StaticStorage.settings.ListLogFile;
            }
            else
            {
                listlog = "./listlog.log";
            }

            string extract_folder;
            if (String.IsNullOrWhiteSpace(StaticStorage.settings.CustomExtractPath))
            {
                if (!Directory.Exists(Path.Combine(StaticStorage.settings.AssetsFolder, "extract")))
                {
                    Directory.CreateDirectory(Path.Combine(StaticStorage.settings.AssetsFolder, "extract"));
                }
                extract_folder = Path.Combine(StaticStorage.settings.AssetsFolder, "extract");
            }
            else
            {
                extract_folder = StaticStorage.settings.CustomExtractPath;
            }

            TextWriteLine("Loading blb index...");

            if (!TestAssetsFolder())
            {
                //this.StartExtracting.Enabled = true;
                MessageBox.Show("Failed to parse bundle_db.blb. Are you sure the path is valid and the file is not corrupt?");
                TextWriteLine("Failed to parse bundle_db.blb. Are you sure the path is valid and the file is not corrupt?");

                //Finished = true;
                return;
            }

            //You don't need to clear them, they're loaded once and done.
            //KnownIndex.Clear();
            //NameIndex.Clear();
            TextWriteLine("Paths and Extensions Updated Successfully");
            listlogger = new StreamWriter(listlog, true);
            if (extract_one && extract_id.Length > 0)
            {
                bundle = new BundleHeader();
                if (!bundle.Load(Path.Combine(StaticStorage.settings.AssetsFolder, extract_id)))
                {
                    MessageBox.Show("Failed to parse bundle header.");
                    TextWriteLine("Failed to parse bundle header.");
                    //Finished = true;
                    return;
                }
                if (StaticStorage.settings.List || StaticStorage.settings.ListData)
                {
                    current_bundle = 0;
                    total_bundle = 1;
                    progress = 0;
                    ListBundle(Path.Combine(StaticStorage.settings.AssetsFolder, extract_id));
                    current_bundle = 1;
                    total_bundle = 1;
                    progress = 100;
                    //Finished = true;
                }
                else
                {
                    current_bundle = 0;
                    total_bundle = 1;
                    progress = 0;
                    TextWriteLine("Extract bundle: " + extract_id);
                    ExtractBundle(extract_id);
                    current_bundle = 1;
                    total_bundle = 1;
                    progress = 100;
                    //Finished = true;
                }
            }
            else
            {
                int TotalBundles = 0;
                int CurrentBundle = 0;
                //float Division = 100.0f;
                TotalBundles = Directory.EnumerateFiles(StaticStorage.settings.AssetsFolder, "*_h.bundle").Count();
                current_bundle = 0;
                total_bundle = (uint)TotalBundles;
                progress = (uint)(((float)CurrentBundle / TotalBundles) * 100);
                foreach (string file in Directory.EnumerateFiles(StaticStorage.settings.AssetsFolder, "*_h.bundle"))
                {
                    string bundle_id = file.Replace("_h.bundle", "");
                    bundle_id = Path.GetFileName(bundle_id);
                    
                    string bundle_path = file.Replace("_h.bundle", "");
                    //Console.WriteLine(bundle_id);
                    bundle = new BundleHeader();
                    TextWriteLine("Loading bundle header " + bundle_id);
                    if (!bundle.Load(bundle_path))
                    {
                        MessageBox.Show("Failed to parse bundle header.");
                        TextWriteLine("Failed to parse bundle header.");
                        //TextWriteLine("Done");
                        return;
                    }
                    if (StaticStorage.settings.List || StaticStorage.settings.ListData)
                    {
                        //TextWriteLine("List Only");
                        ListBundle(bundle_path);
                        CurrentBundle++;
                        current_bundle = (uint)CurrentBundle;
                        total_bundle = (uint)TotalBundles;
                        progress = (uint)(((float)CurrentBundle / TotalBundles) * 100);
                    }
                    else
                    {
                        TextWriteLine("Extracting bundle: " + bundle_id);
                        ExtractBundle(bundle_id);
                        //TextWriteLine("Extraction Done");
                        CurrentBundle++;
                        current_bundle = (uint)CurrentBundle;
                        total_bundle = (uint)TotalBundles;
                        progress = (uint)(((float)CurrentBundle / TotalBundles) * 100);
                    }
                }
            }
            //DialogResult dialogResult = MessageBox.Show("Finished", "Finished", MessageBoxButtons.OK);
            //if (dialogResult == DialogResult.OK)
            //{
                //Finished = true;
            //}
            listlogger.Close();
            listlogger = null;
        }

        public bool TestAssetsFolder()
        {

            if (File.Exists(Path.Combine(StaticStorage.settings.AssetsFolder, "bundle_db.blb")))
            {
                if (!StaticStorage.Index.Load(Path.Combine(StaticStorage.settings.AssetsFolder, "bundle_db.blb")))
                {
                    MessageBox.Show("Failed to load the bundle_db.blb file.");
                    return false;
                }
            }
            else if (File.Exists(Path.Combine(StaticStorage.settings.AssetsFolder, "all.blb")))
            {
                if (!StaticStorage.Index.Load(Path.Combine(StaticStorage.settings.AssetsFolder, "all.blb")))
                {
                    MessageBox.Show("Failed to load the all.blb file.");
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;

        }

        public void ListBundle(string bundle_id, bool write = true)
        {
            foreach (BundleEntry be in bundle.Entries)
            {
                string path = String.Format("unknown_{0:x}.bin", be.Id);
                NameEntry ne = NameIndex.Id2Name(be.Id);
                if (ne != null)
                {
                    string name = KnownIndex.GetPath(ne.Path);
                    string extension = KnownIndex.GetExtension(ne.Extension);
                    if (name != null)
                    {
                        path = name;
                    }
                    else
                    {
                        path = String.Format("{0:x}", ne.Path);
                    }
                    if (ne.Language != 0)
                    {
                        if (StaticStorage.Index.Id2Lang(ne.Language) != null)
                        {
                            string lang_ext = StaticStorage.Known_Index.GetAny(StaticStorage.Index.Id2Lang(ne.Language).Hash);
                            path += String.Format(".{0}", (lang_ext != null ? lang_ext : ne.Language.ToString("x")));
                        }
                        else
                            path += String.Format(".{0:x}", ne.Language);
                    }
                    if (extension != null)
                    {
                        path += String.Format(".{0}", extension);
                    }
                    else
                    {
                        path += String.Format(".{0:x}", ne.Extension);
                    }
                }
                if (write)
                {
                    if (StaticStorage.settings.ListData)
                    {
                        //TextWriteLine(be.Id.ToString() + " - " + be.Length.ToString() + " - " + path + " - " + bundle_id.ToString());
                        listlogger.WriteLine(be.Id.ToString() + " - " + be.Length.ToString() + " - " + path + " - " + Path.GetFileNameWithoutExtension(bundle_id));
                    }
                    else
                    {
                        //TextWriteLine("{0:x} - {1}", ne.Path, path);
                        listlogger.WriteLine(String.Format("{0:x} - {1}", ne.Path, path));
                    }
                }
            }
        }

        public void ExtractBundle(string bundle_id)
        {
            string bundle_file = Path.Combine(StaticStorage.settings.AssetsFolder, bundle_id + ".bundle");
            if (!File.Exists(bundle_file))
            {
                MessageBox.Show("Bundle file does not exist.");
                TextWriteLine("Bundle file does not exist.");
                //Finished = true;
                return;
            }
            using (FileStream fs = new FileStream(bundle_file, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    string extract_folder;
                    if (String.IsNullOrWhiteSpace(StaticStorage.settings.CustomExtractPath))
                    {
                        if (!Directory.Exists(Path.Combine(StaticStorage.settings.AssetsFolder, "extract")))
                        {
                            Directory.CreateDirectory(Path.Combine(StaticStorage.settings.AssetsFolder, "extract"));
                        }
                        extract_folder = Path.Combine(StaticStorage.settings.AssetsFolder, "extract");
                    }
                    else
                    {
                        extract_folder = StaticStorage.settings.CustomExtractPath;
                    }
                    //string file_prefix = "extract/";
                    byte[] data;
                    foreach (BundleEntry be in bundle.Entries)
                    {
                        string path = String.Format("unknown_{0:x}.bin", be.Id);
                        NameEntry ne = NameIndex.Id2Name(be.Id);
                        
                        if (ne != null)
                        {
                            string name = KnownIndex.GetPath(ne.Path);
                            string extension = KnownIndex.GetExtension(ne.Extension);

                            if (StaticStorage.settings.ReplaceTexture && extension.Equals("texture"))
                            {
                                extension += ".dds";
                            }

                            if (StaticStorage.settings.BIKExtensions && extension.Equals("movie"))
                            {
                                extension = extension + ".bik";
                            }

                            if ((StaticStorage.settings.XMLExtensions) && (extension.Equals("material_config") || extension.Equals("object") || extension.Equals("unit") || extension.Equals("gui") || extension.Equals("effect") || extension.Equals("animation_def") || extension.Equals("animation_subset") || extension.Equals("animation_state_machine") || extension.Equals("animation_states") || extension.Equals("post_processor") || extension.Equals("merged_font") || extension.Equals("physic_effect") || extension.Equals("physics_settings") || extension.Equals("network_settings") || extension.Equals("cameras") || extension.Equals("scenes") || extension.Equals("atom_batcher_settings") || extension.Equals("camera_shakes") || extension.Equals("render_template_database")))
                            {
                                extension = extension + ".xml";
                            }
                            if (!StaticStorage.settings.ExtractAll)
                            {
                                switch (extension)
                                {
                                    case "stream":
                                        continue;
                                    case "texture":
                                        continue;
                                    case "movie":
                                        continue;
                                }
                            }
                            if (name != null)
                            {
                                path = name;
                            }
                            else
                            {
                                path = String.Format("{0:x}", ne.Path);
                            }
                            if (ne.Language != 0)
                            {
                                if (StaticStorage.Index.Id2Lang(ne.Language) != null)
                                {
                                    string lang_ext = StaticStorage.Known_Index.GetAny(StaticStorage.Index.Id2Lang(ne.Language).Hash);
                                    path += String.Format(".{0}", (lang_ext != null ? lang_ext : ne.Language.ToString("x")));
                                }
                                else
                                    path += String.Format(".{0:x}", ne.Language);
                            }
                            if (extension != null)
                            {
                                path += String.Format(".{0}", extension);
                            }
                            else
                            {
                                path += String.Format(".{0:x}", ne.Extension);
                            }
                        }
                        string folder = Path.GetDirectoryName(path);
                        if (folder != null && folder.Length != 0)
                        {
                            if (!Directory.Exists(Path.Combine(extract_folder, folder)))
                            {
                                Directory.CreateDirectory(Path.Combine(extract_folder, folder));
                            }
                        }
                        if (be.Length != 0)
                        {
                            using (FileStream os = new FileStream(Path.Combine(extract_folder, path), FileMode.Create, FileAccess.Write))
                            {
                                using (BinaryWriter obr = new BinaryWriter(os))
                                {
                                    fs.Position = be.Address;
                                    if (be.Length == -1)
                                    {
                                        data = br.ReadBytes((int)(fs.Length - fs.Position));
                                        obr.Write(data);
                                    }
                                    else
                                    {
                                        data = br.ReadBytes((int)be.Length);
                                        obr.Write(data);
                                    }
                                }
                            }
                        }
                    }
                }
                //TextWriteLine("Bundle Finish");
            }
        }

    }
}
