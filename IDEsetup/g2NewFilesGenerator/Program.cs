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
                                //if there are any tokens in any of the template files
                                string changedFileName = "";
                                if (atLeastOneToken)
                                {
                                    Console.WriteLine("  CONFIGURE TEMPLATE VARIABLES: \n");
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
                                            Console.WriteLine("  "+fileName);
                                            //for each token that needs to be configured
                                            for (int t = 0; t < pathTokensPair.Value.Count; t++ )
                                            {
                                                //show the tokens that need to be configured
                                                string tokenStr = pathTokensPair.Value[t];
                                                Console.WriteLine(" \t" + tokenStr);
                                                //get just the unique token name (last item in the : separated list)
                                                string uniqueTokenName = getTokenName(tokenStr);
                                                if (!uniqueTokenNames.Contains(uniqueTokenName))
                                                {
                                                    //add the unique token name, if not already in the list
                                                    uniqueTokenNames.Add(uniqueTokenName);
                                                }
                                            }
                                        }
                                    }
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
                                        Console.Write(uniqueTokenNames[u]);
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
                                        for(int t = 0; t < tokens.Count; t++)
                                        {
                                            string tokenKey = tokens[t];
                                            string tokenName = getTokenName(tokenKey);
                                            //get the token value and format the value
                                            string tokenValue = tokenInputLookup[tokenName];
                                            string[] tokenParts = tokenKey.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            string type = tokenParts[0]; type = type.Trim(); type = type.ToLower();
                                            if (type.IndexOf("<<") == 0) { type = type.Substring("<<".Length); }
                                            string casing = tokenParts[1]; casing = casing.Trim(); casing = casing.ToLower();
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
                                                    string firstChar = tokenValue.Substring(0,1);
                                                    string theRest = tokenValue.Substring(1);
                                                    firstChar = firstChar.ToUpper();
                                                    tokenValue = firstChar + theRest;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            //if this is a special token name
                                            switch (type)
                                            {
                                                case "var":
                                                    //replace the tokens with the actual values
                                                    fileContent = fileContent.Replace(tokenKey, tokenValue);
                                                    break;
                                                case "filename":
                                                    //set the name of the file to this token's value
                                                    changedFileName = tokenValue;
                                                    //remove these tokens
                                                    fileContent = fileContent.Replace(tokenKey, "");
                                                    break;
                                                default:
                                                    break;
                                            }
                                            //set the token value
                                            pathOriginalContentLookup[filePath] = fileContent;
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
                                    if (changedFileName != "")
                                    {
                                        //get just the file extension
                                        string fileExt = "";
                                        if (filePath.IndexOf(".") != -1)
                                        {
                                            fileExt = filePath.Substring(filePath.LastIndexOf("."));
                                        }
                                        fileName = changedFileName + fileExt;
                                    }
                                    else
                                    {
                                        fileName = filePath;
                                        if (fileName.IndexOf("\\") != -1)
                                        {
                                            //get just the filename with no path
                                            fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                        }
                                    }
                                    string fileContent = pathContentPair.Value;
                                    string newFilePath = upOneDirPath + "\\" + projFolder + "\\" + fileName;
                                    //if the new file doesn't already exist
                                    if (!File.Exists(newFilePath))
                                    {
                                        //if the file content is NOT blank
                                        fileContent = fileContent.Trim();
                                        if (fileContent.Length > 0)
                                        {
                                            //create the file with its content (maybe changed or maybe not changed and just copied over)
                                            System.IO.File.WriteAllText(newFilePath, fileContent);
                                            Console.WriteLine("  FILE CREATED: \t" + projFolder + "\\" + fileName);
                                            fileCount++;
                                        }
                                        else
                                        {
                                            Console.WriteLine("  FILE SKIP (BLANK): \t" + projFolder + "\\" + fileName);
                                            skippedFileCount++;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("  FILE SKIP (ALREADY EXISTS): \t" + projFolder + "\\" + fileName);
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

        private static string getTokenName(string tokenStr)
        {
            string[] tokenParts = tokenStr.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            string uniqueTokenName = tokenParts[tokenParts.Length - 1];
            uniqueTokenName = uniqueTokenName.Trim(); uniqueTokenName = uniqueTokenName.ToLower();
            uniqueTokenName = uniqueTokenName.Substring(0, uniqueTokenName.LastIndexOf(">>"));
            return uniqueTokenName;
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
