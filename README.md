# Cirqus.TypeScript
Console application used to generated TypeScript typedefinitions for Cirqus commands and views.

# Usage
Please call the tool like this:

    Cirqus.TypeScript <path-to-DLL> <output-directory>

where <path-to-DLL> should point to an assembly containing all of your commands, and <output-directory> should be the directory in which you want the generated 'api.ts' to be put.

# Integrate in Visual Studio
Under 'Project Properties > Build Events', of the project which has Cirqus.TypeScript installed, add the following to 'Post-build event command-line':

    $(TargetDir)Cirqus.TypeScript.exe $(TargetDir)$(TargetFileName) $(SolutionDir)<output-directory>
	
where <output-directory> should be the directory in which you want the generated 'api.ts' to be put.
