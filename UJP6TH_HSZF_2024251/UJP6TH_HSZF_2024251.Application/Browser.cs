using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UJP6TH_HSZF_2024251.Application
{
    public enum BrowserType
    {
        File,
        Folder
    }

    public static class Browser
    {
        public static string GetPath(BrowserType browserType, string fileType = null)
        {
            while (true)
            {
                // Get all drives available on the system
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                List<SearchOptions> driveOptions = new List<SearchOptions>();

                foreach (DriveInfo driveInfo in allDrives)
                {
                    driveOptions.Add(new($"\U0001F4BF {driveInfo.Name}", driveInfo.RootDirectory.FullName));
                }

                // Prompt for drive selection
                var drive = AnsiConsole.Prompt(
                    new SelectionPrompt<SearchOptions>()
                        .AddChoices(driveOptions)
                );

                string currentDirectory = drive.path;

                while (true)
                {
                    List<SearchOptions> folderOptions = new List<SearchOptions>
                    {
                        new($"\U0001F519 Egy szinttel feljebb", "..") // Back option
                    };

                    // get subdirectories in the current directory
                    try
                    {
                        var directories = Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
                        foreach (var dir in directories)
                        {
                            var folderName = Path.GetFileName(dir);
                            folderOptions.Add(new($"\U0001F4C1 {folderName}", dir));
                        }
                        
                        // add confirmation if looking for folder
                        if (browserType == BrowserType.Folder)
                        {
                            folderOptions.Add(new($"\U0001F4BE Kiválasztás", currentDirectory));
                        }

                        if (browserType == BrowserType.File && fileType != null)
                        {
                            var files = Directory.GetFiles(currentDirectory, $"*.{fileType}", SearchOption.TopDirectoryOnly);
                            foreach (var file in files)
                            {
                                var fileName = Path.GetFileName(file);
                                folderOptions.Add(new($"\U0001F4DD {fileName}", file));
                            }
                        }
                    }

                    // disaster prevention if no access
                    catch (UnauthorizedAccessException)
                    {
                        AnsiConsole.WriteLine($"Nincs jogosultság ezen mappa megnyitásához: {currentDirectory}");
                        break;
                    }

                    // prompt for selection
                    var selectedOption = AnsiConsole.Prompt(
                        new SelectionPrompt<SearchOptions>()
                            .Title($"Jelenlegi hely: [green]{currentDirectory}[/]")
                            .AddChoices(folderOptions)
                    );

                    // going up in directory
                    if (selectedOption.path == "..")
                    {
                        var parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                        if (parentDirectory == null)
                        {
                            // if no parent, go back to disk selection
                            break;
                        }

                        currentDirectory = parentDirectory;
                        continue;
                    }

                    // handle return based on browser mode
                    if (browserType == BrowserType.File && File.Exists(selectedOption.path))
                    {
                        return selectedOption.path; // return file path
                    }

                    // Handle folder selection if in Folder mode
                    if (browserType == BrowserType.Folder && selectedOption.path == currentDirectory)
                    {
                        return currentDirectory; // return folder path
                    }

                    currentDirectory = selectedOption.path;
                }
            }
        }
    }
}
