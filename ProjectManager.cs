using System.Diagnostics;
using System.Xml.Linq;
using Flow.Launcher.Plugin;

namespace RiderWorkFlow;

public class ProjectManager
{
    private const string IcoPath = "Images/Icon.png";
    private const string IDEName = "rider";
    private const string UserHomePlaceholder = "$USER_HOME$";
    private readonly string _optionPath;

    public ProjectManager()
    {
        _optionPath = GetOptionPath();
    }

    public List<Result> GetResultProjects(string projectName)
    {
        return GetProjects(projectName)
            .Select(e => new Result
            {
                Title = e.Name,
                SubTitle = e.ProjectPath,
                IcoPath = IcoPath,
                Action = (c) => OpenProject(e.ProjectPath)
            }).ToList();
    }

    private List<ProjectModel> GetProjects(string projectName)
    {
        var xmlContent = File.ReadAllText(_optionPath);
        var xmlDoc = XDocument.Parse(xmlContent);
        var names = xmlDoc.Descendants("entry")
            .Select(e => new ProjectModel { ProjectPath = FixPathForWindows(ReplaceUserHome(e.Attribute("key")!.Value)) })
            .Where(e => e.Name.Contains(projectName, StringComparison.CurrentCultureIgnoreCase)).ToList();

        return names;
    }

    private static string ReplaceUserHome(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        return path.Replace(UserHomePlaceholder, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    private static string FixPathForWindows(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        return path.Replace("/", "\\");
    }

    private static bool OpenProject(string solutionPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = solutionPath,
            UseShellExecute = true
        };

        try
        {
            Process.Start(processStartInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetOptionPath()
    {
        var applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\JetBrains\";
        const string optionsFilePath = @"\options\recentSolutions.xml";

        var riderVersion = Directory.GetDirectories(applicationDataFolder)
            .Select(Path.GetFileName)
            .FirstOrDefault(e => e != null && e.Contains(IDEName, StringComparison.CurrentCultureIgnoreCase));

        return applicationDataFolder + riderVersion + optionsFilePath;
    }
}