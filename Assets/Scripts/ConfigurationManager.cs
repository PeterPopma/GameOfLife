using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour 
{
    private string dirPath = "";

    public Configuration Load(string fileName)
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dirPath, fileName);
        Configuration loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<Configuration>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load file at path: "
                        + fullPath + " and backup did not work.\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(Configuration configuration, string fileName)
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dirPath, fileName);
        try
        {
            // create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(configuration);

            // write the serialized data to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Delete(string fileName)
    {
        string fullPath = Path.Combine(dirPath, fileName);

        // ensure the data file exists at this path before deleting the directory
        if (File.Exists(fullPath))
        {
            // delete the profile folder and everything within it
            Directory.Delete(Path.GetDirectoryName(fullPath), true);
        }
        else
        {
            Debug.LogWarning("Tried to delete configuration data, but data was not found at path: " + fullPath);
        }
    }

}
