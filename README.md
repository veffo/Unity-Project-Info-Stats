<p align="center">
    <a href="#">
        <img width="150" src="https://github.com/veffo/Unity-Project-Info-Stats/blob/main/_assets/media/logo.png" alt="logo" />
    </a>
</p>

<p align="center">
    <a href="#"><img alt="license MIT" src="https://img.shields.io/badge/license-MIT-8dbb05.svg" /></a>
    <a href="#"><img alt="Stars" src="https://img.shields.io/github/stars/veffo/gitignore?style=flat-square" /></a>
    <a href="#"><img alt="Forks" src="https://img.shields.io/github/forks/veffo/gitignore?style=flat-square" /></a>
</p>

<p align="center">
    This is an analytics files tool for the Unity editor.
    <br />
    <a href="mailto:q.6110@mail.ru">Report bug</a>
    ·
    <a href="mailto:q.6110@mail.ru">Request feature</a>
</p>

<div id="user-content-toc">
    <ul align="center" style="list-style: none;">
        <summary>
            <h1>
                Project Info Stats
            </h1>
        </summary>
    </ul>
</div>

## Table of Contents

<details>
    <summary>
        <b>Expand list</b>
    </summary>
    <ul>
        <li>
            <a href="#about-the-project">
                About The Project
            </a>
        </li>
        <li>
            <a href="#demo">
                Demo
            </a>
        </li>
        <li>
            <a href="#features">
                Features
            </a>
        </li>
        <li>
            <a href="#installation">
                Installation
            </a>
        </li>
        <li>
            <a href="#usage">
                Usage
            </a>
        </li>
        <li>
            <a href="#example-class">
                Example class
            </a>
        </li>
        <li>
            <a href="#conclusion">
                Conclusion
            </a>
        </li>
        <li>
            <a href="#requirements">
                Requirements
            </a>
        </li>
        <li>
            <a href="#documentation">
                Documentation
            </a>
        </li>
        <li>
            <a href="#support">
                Support
            </a>
        </li>
        <li>
            <a href="#license">
                License
            </a>
        </li>
    </ul>
</details>

## About The Project

This is an advanced analytics tool for the Unity editor, designed to optimize
your workflow by collecting and analyzing available data and files, and for
convenience, the tool also has a real-time search of your project data.

No third-party tools are used, or are required to run this package.

## Demo

<p>
    <a href="#">
        <img src="https://github.com/veffo/Unity-Project-Info-Stats/blob/main/_assets/media/demo.webp" alt="demo" />
    </a>
</p>

## Features

<ul>
    <li>
        Generate detailed overview visual.
    </li>
    <li>
        Easy customization and usability.
    </li>
    <li>
        Improve usability with subtle modifications.
    </li>
    <li>
        View fields to easily reference stats.
    </li>
    <li>
        Fast-and-easy implementation.
    </li>
    <li>
        Performance.
    </li>
    <li>
        CSV export for further analysis.
    </li>
    <li>
        HTML export for further analysis.
    </li>
    <li>
        Dynamic search.
    </li>
</ul>

## Installation

### Option 1: Using Unity Package

1. Download the `.unitypackage` file from latest release.
2. In Unity Editor go to **Assets → Import Package → Custom Package**.
3. Select the downloaded package file.
4. Make sure all files are selected and click Import.

### Option 2: Using URL Package

1. Open your Unity project and navigate to **Window → Package Manager**.
2. Click the **"+"** icon on the top-left, then choose **Add package from git URL**.
3. Enter the Git URL: `https://github.com/veffo/Unity-Project-Info-Stats.git` and click **"Add"**.

```shell
https://github.com/veffo/Unity-Project-Info-Stats.git
```

4. The package will now be installed and ready for use within your project.

<p>
    <a href="#">
        <img src="https://github.com/veffo/Unity-Project-Info-Stats/blob/main/_assets/media/install/url_package.webp" alt="Using url package" />
    </a>
</p>

### Option 3: Manual Installation

1. Clone or download the repository:

```shell
git clone https://github.com/veffo/Unity-Project-Info-Stats.git
```
2. Copy the `ProjectStatsTools` folder to the `Assets/Editor` folder in your Unity project (create the Editor folder if it doesn't exist).
3. Navigate to the `ProjectInfoStats/Scripts` folder and explore the scripts.

Main build are in `Assets/Editor/ProjectInfoStats`.

## Usage

To start using the **Project Info Stats** in your Unity project, first ensure it is properly installed via.
Once installed, you can open the window by navigating to Open the Package **"Tools" → "Project Info Stats"**
in your Unity Editor. He allows you to dynamically inspect your project data.
This creates a log file that can be look or load in this plugin.

<p>
    <a href="#">
        <img src="https://github.com/veffo/Unity-Project-Info-Stats/blob/main/_assets/media/usage/step_1.png" alt="step 1" />
    </a>
</p>

<p>
    <a href="#">
        <img src="https://github.com/veffo/Unity-Project-Info-Stats/blob/main/_assets/media/usage/step_2.png" alt="step 2" />
    </a>
</p>

**Navigating**: Toggle the visibility of fields, allowing for a customized view that suits your current needs.

**Dynamic Search**: Offers the capability to search through files, making it easier to navigate and pinpoint specific data.

## Example class

Now let's give an example of a class for using data, implemented in the `ProjectInfoStatsTools` namespace:

```csharp
    namespace ProjectInfoStatsTools {
        [Serializable]
        public class ScriptStats {
            public string projectName, projectVersion, targetPlatform = null;
            public int TotalFiles, TotalLines, TotalEmptyLines, TotalCommentLines = 0;
            public float TotalAverageLines, TotalAssetSizes = 0;
            public int TotalClasses, TotalMethods, TotalVariables, TotalNamespaces, TotalInterface, TotalEnum, TotalStruct = 0;
            public int TotalPrefabs, TotalMaterials, TotalScenes, TotalTextures, TotalAudioClips, TotalVideoClips, TotalShaders, TotalAnimationClips = 0;
            public List<FileStat> FileStats = new List<FileStat>();
            public string LastAnalyzed;
        }
    }
```

## Conclusion

The Project Info Stats tool is particularly useful for developers who need to manage game settings,
optimizate files, and save data during development. Integrate is tool into your Unity workflow,
customize them to fit your needs, and explore their capabilities for efficiently analyzing files project.

## Requirements

<ul>
    <li>
        Git
    </li>
    <li>
        Unity
    </li>
</ul>

## Documentation

Resources for information on how to work with `Project Info Stats` and how to use:

- The <a href="https://github.com/veffo/Unity-Project-Info-Stats/blob/main/ProjectStatsTools/documentation/documentation.pdf" target="_blank">Project Info Stats</a> manual page.

## Support

For all questions, please contact us by email: <a href="mailto:q.6110@mail.ru">q.6110@mail.ru</a>

## License

This project is open-sourced software licensed under the <a href="https://opensource.org/license/MIT" target="_blank">MIT license</a>.<br/>
Distributed under the <a href="https://opensource.org/license/MIT" target="_blank">MIT license</a>. See <a href="https://opensource.org/license/MIT" target="_blank">MIT license</a> for more information.
