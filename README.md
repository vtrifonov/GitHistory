# Git History Tool
Small tool getting history for a path in a given local git repository and exporting it using [RazorTemplates](https://github.com/volkovku/RazorTemplates)

The tool get all the changes between revisions or to a revision of a local git repository and pass the commits to a Razor template which then outputs the result in a given file. The commits can be filtered by a given pat in the repository.

# Usage

Usage: _GitHistory.App.exe [-gitRepo] <String> [-templateFile] <String> [-outputFile] <String> [-endRevision <String>] [-gitInstallationFolder <String>] [-includeMerges] [-pageTitle <String>] [-pathInRepo <String>] [-startRevision <String>]_

To use the tool you can pass the following arguments:

* **gitRepo**(or just **r**) - The local path to the git repository
* **templateFile**(or just **t**) - The path to the Razor template file that will be used for tansforming the list of commits
* **outputFile**(or just **o**) - The path to the file where the result content will be saved
* **startRevision**(or **r** or **revision**) - This argument is optional and is needed if you want to get the changes in a range or to a revision
* **endRevision**(or just **er**) - Optional argument that specifies the end revision
* **includeMerges**(or just **im**) - Whether or not the merge commits to be included in the output
* **gitInstallationFolder**(or just **gi**) - The path to the git executable if it is not set in the Path environment variable
* **pathInRepo**(or just **p**) - The relative path inside the local repository where you want to get the changes for
* **pageTitle**(or just **pt**) - This can be used for presentation purposes only. Can be passed to the command line and then output in the result

You can find example usage(**example.bat**) and razor(**example.razor**) files inside the project folder
