using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
namespace g2NewFilesGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string newfile_templates = "newfile_templates";
            //bool foundDir = false;
            //get the current directory of this .exe file
            string currentDirPath = AppDomain.CurrentDomain.BaseDirectory;
            //get the new current directory (up one level)
            string upOneDirPath = currentDirPath;
            string projFolder = upOneDirPath;
            //trim off the trailing "\" if it is there
            if (upOneDirPath.LastIndexOf("\\") == upOneDirPath.Length - 1)
            {
                //trim off "\"
                upOneDirPath = upOneDirPath.Substring(0, upOneDirPath.Length - 1);
                projFolder = upOneDirPath;
            }
            //if upOneDirPath still contains "\"
            if (upOneDirPath.LastIndexOf("\\") != -1)
            {
                //trim off the last folder in the currentDirPath
                upOneDirPath = upOneDirPath.Substring(0, upOneDirPath.LastIndexOf("\\"));
                //get JUST the project folder without the path
                projFolder = projFolder.Substring(projFolder.LastIndexOf("\\") + 1);
            }

            string templateRootPath = upOneDirPath + "\\" + newfile_templates;
            if (Directory.Exists(templateRootPath))
            {
                //detect all of the folder paths that contain files
                List<string> templateOptions = new List<string>();
                List<string> templateOutputOptions = new List<string>();
                loadDirsWithFiles(templateRootPath, ref templateOptions);
                //if there is at least one template
                if (templateOptions.Count > 0)
                {
                    Console.WriteLine("\n  Template files will be copied from " + newfile_templates + " --> " + projFolder + "\n");
                    Console.WriteLine("  Choose template by [number]: ");

                    int chosenTemplateIndex = -1; bool validOption = false;
                    //if there is more than one template option
                    if (templateOptions.Count > 1)
                    {
                        //for each folder path that contains file(s)
                        for (int t = 0; t < templateOptions.Count; t++)
                        {
                            //show the template option and build the display string for this option
                            showTemplateOption(t, newfile_templates, templateOptions, ref templateOutputOptions);
                        }
                        //allow user to choose template and accept key input
                        Console.Write("\n  Choose template: ");
                        string line = Console.ReadLine(); line = line.Trim();
                        if (int.TryParse(line, out chosenTemplateIndex))
                        {
                            //this IS an int...
                            validOption = true;
                        }
                        else //this is NOT an int
                        {
                            chosenTemplateIndex = -1;
                            Console.WriteLine("  Error, \"" + line + "\" is not an integer.");
                        }
                    }
                    else //only one template option
                    {
                        //just use the first index
                        chosenTemplateIndex = 0;
                        validOption = true;
                        //show the template option and build the display string for this option
                        showTemplateOption(0, newfile_templates, templateOptions, ref templateOutputOptions);
                    }
                    //if input is valid for template option
                    if (validOption)
                    {
                        //if the integer is NOT too high
                        if (chosenTemplateIndex < templateOptions.Count)
                        {
                            //if the integer is NOT too low
                            if (chosenTemplateIndex > -1)
                            {
                                Console.Clear();
                                Console.WriteLine("\n  TEMPLATE: \n");
                                Console.WriteLine(templateOutputOptions[chosenTemplateIndex]);
                                Console.WriteLine("\n  ==========================================\n");
                                //Dictionary<[filePath], [originalFileContent]>
                                Dictionary<string, string> pathOriginalContentLookup = new Dictionary<string, string>();
                                //Dictionary<[filePath], Dictionary<[tokenKey], [tokenInputValue]>>
                                Dictionary<string, List<string>> pathTokensLookup = new Dictionary<string, List<string>>();
                                bool atLeastOneToken = false;
                                //get the files in this directory
                                DirectoryInfo dir = new DirectoryInfo(templateOptions[chosenTemplateIndex]);
                                FileInfo[] files = dir.GetFiles();
                                foreach (FileInfo file in files)
                                {
                                    //get the file contents
                                    string contents = System.IO.File.ReadAllText(file.FullName);
                                    //store the file path and original content
                                    pathOriginalContentLookup.Add(file.FullName, contents);
                                    //store the tokens for this file's content
                                    List<string> tokens = getTokensFromContent(contents);
                                    pathTokensLookup.Add(file.FullName, tokens);
                                    if (tokens.Count > 0)
                                    {
                                        atLeastOneToken = true;
                                    }
                                }
                                //Dictionary<[filePath], [changedFileName]>
                                Dictionary<string, string> changedFileNames = new Dictionary<string,string>();
                                //Dictionary<[filePath], [changedFileDir]>
                                Dictionary<string, string> changedFileDirs = new Dictionary<string, string>(); 
                                //if there are any tokens in any of the template files
                                if (atLeastOneToken)
                                {
                                    string configVarsMsg = "";
                                    configVarsMsg += "  CONFIGURE TEMPLATE VARIABLES: \n\n";
                                    //List<[tokenName]>
                                    List<string> uniqueTokenNames = new List<string>();
                                    //for each file 
                                    foreach (KeyValuePair<string, List<string>> pathTokensPair in pathTokensLookup)
                                    {
                                        //if this file has any tokens that need to be configured
                                        if (pathTokensPair.Value.Count > 0)
                                        {
                                            //get just the filename without the path
                                            string fileName = pathTokensPair.Key;
                                            if (fileName.IndexOf("\\") != -1)
                                            {
                                                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                            }
                                            configVarsMsg += "  "+fileName + "\n";
                                            //for each token that needs to be configured
                                            for (int t = 0; t < pathTokensPair.Value.Count; t++ )
                                            {
                                                //show the tokens that need to be configured
                                                string tokenStr = pathTokensPair.Value[t];
                                                configVarsMsg += " \t" + tokenStr + "\n";
                                                //get just the unique token name (last item in the : separated list)
                                                string uniqueTokenName = getTokenPart("name", tokenStr);
                                                //if this is not a blank token
                                                if (uniqueTokenName != ".")
                                                {
                                                    if (!uniqueTokenNames.Contains(uniqueTokenName))
                                                    {
                                                        //add the unique token name, if not already in the list
                                                        uniqueTokenNames.Add(uniqueTokenName);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //if there are any NON blank token names (that were represented by a dot, .)
                                    if (uniqueTokenNames.Count > 0)
                                    {
                                        //show the configure variables message
                                        Console.WriteLine(configVarsMsg);
                                        //if there is only one token
                                        if (uniqueTokenNames.Count == 1)
                                        {
                                            Console.Write("\n  Just " + uniqueTokenNames.Count + " unique token --> ");
                                        }
                                        else
                                        {
                                            Console.Write("\n  " + uniqueTokenNames.Count + " unique tokens --> ");
                                        }
                                        //list the unique variable names
                                        for (int u = 0; u < uniqueTokenNames.Count; u++)
                                        {
                                            Console.Write("\"" + uniqueTokenNames[u] + "\"");
                                            if (u + 1 != uniqueTokenNames.Count)
                                            {
                                                Console.Write(", ");
                                            }
                                            else
                                            {
                                                Console.WriteLine("\n");
                                            }
                                        }
                                        Console.WriteLine("  -------------------------------------------------------\n");
                                        //for each token require input
                                        Dictionary<string, string> tokenInputLookup = new Dictionary<string, string>();
                                        for (int i = 0; i < uniqueTokenNames.Count; i++)
                                        {
                                            //get the value for this token from the user
                                            Console.Write("  Enter --> " + uniqueTokenNames[i] + ": ");
                                            string line = Console.ReadLine(); line = line.Trim();
                                            tokenInputLookup.Add(uniqueTokenNames[i], line);
                                        }

                                        Console.WriteLine("\n  OK, got it. Hit any key to build..."); Console.ReadKey();

                                        //for each template file
                                        foreach (KeyValuePair<string, List<string>> pathPair in pathTokensLookup)
                                        {
                                            string filePath = pathPair.Key;
                                            string fileContent = pathOriginalContentLookup[filePath];
                                            //Dictionary<[tokenKey], [tokenInputValue]>
                                            List<string> tokens = pathPair.Value;
                                            //for each token in the file
                                            for (int t = 0; t < tokens.Count; t++)
                                            {
                                                //get the token key, eg: <<type:casing:name>>
                                                string tokenKey = tokens[t];
                                                //split the token key parts up
                                                string[] tokenParts = tokenKey.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                //get the token name
                                                string tokenName = getTokenPart("name", tokenParts);
                                                //get the token type
                                                string type = getTokenPart("type", tokenParts);
                                                //get the token casing
                                                string casing = getTokenPart("casing", tokenParts);
                                                //if not a blank tokenName, represented by a dot, .
                                                string tokenValue = "";
                                                if (tokenName != ".")
                                                {
                                                    //get the token value... the value is formatted based on the different token parts, eg: casing
                                                    tokenValue = tokenInputLookup[tokenName];
                                                    switch (casing)
                                                    {
                                                        case "uppercase":
                                                            tokenValue = tokenValue.ToUpper();
                                                            break;
                                                        case "lowercase":
                                                            tokenValue = tokenValue.ToLower();
                                                            break;
                                                        case "capitalize":
                                                            tokenValue = tokenValue.ToLower();
                                                            string firstChar = tokenValue.Substring(0, 1);
                                                            string theRest = tokenValue.Substring(1);
                                                            firstChar = firstChar.ToUpper();
                                                            tokenValue = firstChar + theRest;
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                //if this is a special token name
                                                switch (type)
                                                {
                                                    case "var":
                                                        //replace the tokens with the actual values
                                                        fileContent = fileContent.Replace(tokenKey, tokenValue);
                                                        break;
                                                    case "filename":
                                                        //remove these tokens from the file content
                                                        fileContent = fileContent.Replace(tokenKey, "");
                                                        //if this file doesn't already have a designated changed name
                                                        if (!changedFileNames.ContainsKey(filePath))
                                                        {
                                                            //if there is a specified file name (other than the existing template file's name)
                                                            if (tokenValue != "" && tokenValue != ".")
                                                            {
                                                                //set the new name of the file
                                                                changedFileNames.Add(filePath, tokenValue);
                                                            }
                                                        }
                                                        //since this token specifies a filename, it may also specify the root directory for the file (if not, changedFileDir = "")...
                                                        //if this file doesn't already have a designated changed sub directory
                                                        if (!changedFileDirs.ContainsKey(filePath))
                                                        {
                                                            //if there is a specified directory in the tokenParts
                                                            string changedDir = getTokenPart("dir", tokenParts);
                                                            if (changedDir != "" && changedDir != ".")
                                                            {
                                                                //set the new sub directory of the file
                                                                changedFileDirs.Add(filePath, changedDir);
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                //set the token value
                                                pathOriginalContentLookup[filePath] = fileContent;
                                            }
                                        }
                                    }
                                }
                                Console.WriteLine("  -------------------------------------------------------");
                                //for each file to create
                                int fileCount = 0; int skippedFileCount = 0;
                                foreach (KeyValuePair<string, string> pathContentPair in pathOriginalContentLookup)
                                {
                                    string filePath = pathContentPair.Key;
                                    string fileName = "";
                                    //if changing the file name
                                    if (changedFileNames.ContainsKey(filePath))
                                    {
                                        //get just the file extension
                                        string fileExt = "";
                                        if (filePath.IndexOf(".") != -1)
                                        {
                                            fileExt = filePath.Substring(filePath.LastIndexOf("."));
                                        }
                                        fileName = changedFileNames[filePath] + fileExt;
                                    }
                                    else //use same filename as the original template file
                                    {
                                        fileName = filePath;
                                        if (fileName.IndexOf("\\") != -1)
                                        {
                                            //get just the filename with no path
                                            fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                        }
                                    }
                                    //if changing the file directory (under the current project directory)
                                    string changedFileDir = "";
                                    if (changedFileDirs.ContainsKey(filePath))
                                    {
                                        changedFileDir = changedFileDirs[filePath];
                                        //make sure each directory exists... create them if they don't
                                        string[] dirs = changedFileDir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                        string currentDir = upOneDirPath + "\\" + projFolder + "\\";
                                        for (int d = 0; d < dirs.Length; d++)
                                        {
                                            //if this directory doesn't exist
                                            currentDir += dirs[d] + "\\";
                                            if (!Directory.Exists(currentDir))
                                            {
                                                //create the directory
                                                Directory.CreateDirectory(currentDir);
                                            }
                                        }
                                        //append the final \\ at the end of the directory path
                                        changedFileDir += "\\";
                                    }
                                    //get the new file content
                                    string fileContent = pathContentPair.Value;
                                    string newFilePath = upOneDirPath + "\\" + projFolder + "\\" + changedFileDir + fileName;
                                    //if the new file doesn't already exist
                                    if (!File.Exists(newFilePath))
                                    {
                                        //if the file content is NOT blank
                                        fileContent = fileContent.Trim();
                                        if (fileContent.Length > 0)
                                        {
                                            //create the file with its content (maybe changed or maybe not changed and just copied over)
                                            System.IO.File.WriteAllText(newFilePath, fileContent);
                                            Console.WriteLine("  FILE CREATED: \t" + projFolder + "\\" + changedFileDir + fileName);
                                            fileCount++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("  FILE SKIP (BLANK): \t" + projFolder + "\\" + changedFileDir + fileName);
                                            skippedFileCount++;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("  FILE SKIP (ALREADY EXISTS): \t" + projFolder + "\\" + changedFileDir + fileName);
                                        skippedFileCount++;
                                    }
                                }
                                Console.WriteLine("  -------------------------------------------------------\n");
                                Console.WriteLine("\n  Done.\n  Created files: (" + fileCount + ") || Skipped files: (" + skippedFileCount + ")");
                            }
                            else
                            {
                                Console.WriteLine("  Error, \"" + chosenTemplateIndex + "\" is too low.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("  Error, there are only " + templateOptions.Count + " options. \"" + chosenTemplateIndex + "\" is too high.");
                        }
                    }
                }
                else //no templates
                {
                    Console.WriteLine("\n  There are no templates in \"../" + newfile_templates + "\".");
                    Console.WriteLine("  " + templateRootPath);
                }
            }
            else
            {
                Console.WriteLine("  Uh oh; the root template folder, \"../" + newfile_templates + "\" was not found:");
                Console.WriteLine(templateRootPath);
            }
            Console.WriteLine("\n\n  Press any key to close...");
            Console.ReadKey();
        }

        private static string getTokenPart(string partKey, string tokenStr)
        {
            //get the token parts, eg: <<type:casing:name>>
            string[] tokenParts = tokenStr.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return getTokenPart(partKey, tokenParts);
        }
        private static string getTokenPart(string partKey, string[] tokenParts)
        {
            string returnStr = "";
            //return a different part depending on the given part key
            switch(partKey)
            {
                case "type":
                    //type is always first part
                    string type = tokenParts[0];
                    //if token name contains >>
                    if (type.Contains("<<"))
                    {
                        //remove starting <<
                        type = type.Substring(">>".Length);
                    }
                    //always trim and lowercase token type
                    type = type.Trim(); type = type.ToLower();
                    returnStr = type;
                    break;
                case "casing":
                    //token casing is always second part
                    string casing = tokenParts[1];
                    //always trim and lowercase token casing
                    casing = casing.Trim(); casing = casing.ToLower();
                    returnStr = casing;
                    break;
                case "dir":
                    //if there are more than 3 token parts
                    if (tokenParts.Length > 3)
                    {
                        //recursively get type
                        string tokenType = getTokenPart("type", tokenParts);
                        //if this type is a "filename"
                        if (tokenType == "filename")
                        {
                            //token directory is always second-to-last part, eg: <<filename:lowercase:folder/path:filename>>
                            string dir = tokenParts[tokenParts.Length - 2];
                            //always trim token dir
                            dir = dir.Trim();
                            returnStr = dir;
                            //normalize the directory separators
                            returnStr = returnStr.Replace("\\", "/");
                            returnStr = returnStr.Replace("///", "/");
                            returnStr = returnStr.Replace("//", "/");
                            returnStr = returnStr.Replace("/", "\\");
                            //if this dir path contains a separtor
                            if (returnStr.Contains("\\"))
                            {
                                //cannot end with \\
                                if (returnStr.LastIndexOf("\\") == returnStr.Length - 1)
                                {
                                    //trim off ending \\
                                    returnStr = returnStr.Substring(0, returnStr.Length - 1);
                                }
                                //cannot start with \\
                                if (returnStr.IndexOf("\\") == 0)
                                {
                                    //trim off starting \\
                                    returnStr = returnStr.Substring(1);
                                }
                            }
                        }
                    }
                    break;
                case "name":
                    //token name is always last part
                    string uniqueTokenName = tokenParts[tokenParts.Length - 1];
                    //if token name contains >>
                    if (uniqueTokenName.Contains(">>"))
                    {
                        //remove trailing >>
                        uniqueTokenName = uniqueTokenName.Substring(0, uniqueTokenName.LastIndexOf(">>"));
                    }
                    //always trim and lowercase token name
                    uniqueTokenName = uniqueTokenName.Trim(); uniqueTokenName = uniqueTokenName.ToLower();
                    returnStr = uniqueTokenName;
                    break;

            }
            return returnStr;
        }

        private static void showTemplateOption(int index, string newfile_templates, List<string> templateOptions, ref List<string> templateOutputOptions)
        {
            //get the path for this option
            string templateOption = templateOptions[index];
            //remove first part of the path
            templateOption = templateOption.Substring(templateOption.IndexOf("\\" + newfile_templates) + 1);
            //write template option
            Console.WriteLine("-----------------------------------------------");
            string outputStr = "";
            outputStr += "  " + index + "\t " + templateOption;
            //print the files inside this folder
            DirectoryInfo dir = new DirectoryInfo(templateOptions[index]);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                //print file
                outputStr += "\n\t\t " + file.Name;
            }
            templateOutputOptions.Add(outputStr);
            //print
            Console.WriteLine(outputStr);
        }

        private static void loadDirsWithFiles(string rootPath, ref List<string> templateOptions)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + rootPath);
            }

            // Get the files in the directory
            FileInfo[] files = dir.GetFiles();
            //if there are any files in this directory
            if (files.Length > 0)
            {
                //if this directory is not already in the list
                if (!templateOptions.Contains(dir.FullName))
                {
                    //get the directory path
                    templateOptions.Add(dir.FullName);
                }
            }

            //for each sub directory
            foreach (DirectoryInfo subdir in dirs)
            {
                //find out what sub directories also have files
                loadDirsWithFiles(subdir.FullName, ref templateOptions);
            }
        }

        private static List<string> getTokensFromContent(string contents)
        {
            //Dictionary<[tokenKey], [blankToStoreTokenValue]>
            List<string> tokens = new List<string>();

            //what are the different possible token type starting strings?
            List<string> tokenStartTags = new List<string>();
            tokenStartTags.Add("var");
            tokenStartTags.Add("filename");

            //if the file content contains <
            string[] splitByCarrot = contents.Split(new char[]{'<'}, StringSplitOptions.RemoveEmptyEntries);
            if (splitByCarrot.Length > 0)
            {
                //for each string starting with <
                for (int c = 0; c < splitByCarrot.Length; c++)
                {
                    //if the string starts with <<
                    string str = splitByCarrot[c];
                    //if the string contains >>
                    if (str.Contains(">>"))
                    {
                        //get just the string between the << and >>
                        str = str.Substring(0, str.IndexOf(">>") + ">>".Length);
                        //if the string contains :
                        if (str.Contains(":"))
                        {
                            //get the token parts
                            string[] tokenParts = str.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            //if the first token part is a type listed in tokenStartTags
                            if(tokenStartTags.Contains(tokenParts[0]))
                            {
                                //if there are at least three parts to the token
                                if (tokenParts.Length > 2)
                                {
                                    //if contents contains this string starting with <<
                                    str = "<<" + str;
                                    if (contents.Contains(str))
                                    {
                                        //if this key is not already in the list
                                        if (!tokens.Contains(str))
                                        {
                                            //add the possible token to the allTokens list
                                            tokens.Add(str);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return tokens;
        }
    }
}
