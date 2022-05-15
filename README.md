# Maestro
Software architecture tools that simplify changing code that "wasn't designed to do that"

## Short overview of architecture and definition of terms

We consider architecture to consist of three high level areas:

1. **Components**: The basic building blocks, usually individual classes/interfaces
2. **Composition**: How components are used together and how they are created/destroyed
3. **Organization**: How you find your components, this is almost always a file system explorer like solution explorer in Visual Studio

# Tools List

## SolutionExplorer++

**Short Description**: Puts files that are frequently modified together near each other in solution explorer

**Detailed Description**: A lot of development time is wasted searching through solution explorer because it shows files based on how they are stored on the file system rather than how they are likely to be used by the programmer. Often times files that typically modified together are not close to each other on the file system and may not be easy to find without a detailed knowledge of the architecture of the code.

Solution explorer is basically like a database for files that has a fixed structure where a file can only appear in a single place. To improve this we create an "index" for a file based on the git history. Files that are typically modified along with a given file will now appear directly nested underneath it as links. This way it is faster to find the files that you are likely to actually use.

**Requirements**:
1. The .git folder for the solution should be in the same directory as the .sln file for that solution otherwise the extension will do nothing
2. Because of the way visual studio works files need to be created for the links so *.link files should be added to your .gitignore. Also you should ignore the file maestrohistory.txt that will exist in the same folder as your .sln file.
