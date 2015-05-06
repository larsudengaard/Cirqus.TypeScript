# Cirqus.TypeScript
Console application used to generated TypeScript typedefinitions for Cirqus commands and views.

# Usage
Please call the tool like this:

    Cirqus.TypeScript <path-to-DLL> <output-file>

where `<path-to-DLL>` should point to an assembly containing all of your commands, and `<output-file>` should be the file in which you want the generated typescript to be put.

# Integrate in Visual Studio
Under 'Project Properties > Build Events', of the project which has Cirqus.TypeScript installed, add the following to 'Post-build event command-line':

    $(TargetDir)Cirqus.TypeScript.exe $(TargetDir)$(TargetFileName) $(SolutionDir)<output-file>
	
where `<output-file>` should be the file in which you want the generated output to go.
