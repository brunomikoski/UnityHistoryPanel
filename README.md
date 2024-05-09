# Unity Editor History panel


<p align="center">
    <a href="https://github.com/brunomikoski/UnityHistoryPanel/blob/master/LICENSE.md">
		<img alt="GitHub license" src ="https://img.shields.io/github/license/brunomikoski/UnityHistoryPanel" />
	</a>

</p> 
<p align="center">
    <a href="https://openupm.com/packages/com.brunomikoski.editorhistorypanel/">
        <img src="https://img.shields.io/npm/v/com.brunomikoski.editorhistorypanel?label=openupm&amp;registry_uri=https://package.openupm.com" />
    </a>

  <a href="https://github.com/brunomikoski/UnityHistoryPanel/issues">
     <img alt="GitHub issues" src ="https://img.shields.io/github/issues/brunomikoski/UnityHistoryPanel" />
  </a>

  <a href="https://github.com/brunomikoski/UnityHistoryPanel/pulls">
   <img alt="GitHub pull requests" src ="https://img.shields.io/github/issues-pr/brunomikoski/UnityHistoryPanel" />
  </a>

  <img alt="GitHub last commit" src ="https://img.shields.io/github/last-commit/brunomikoski/UnityHistoryPanel" />
</p>

<p align="center">
    	<a href="https://github.com/brunomikoski">
        	<img alt="GitHub followers" src="https://img.shields.io/github/followers/brunomikoski?style=social">
	</a>	
	<a href="https://twitter.com/brunomikoski">
		<img alt="Twitter Follow" src="https://img.shields.io/twitter/follow/brunomikoski?style=social">
	</a>
</p>



## Features
- Don't need to be focuses to get shortcuts, just open and keep with something, like inspector
- Keep history between Editor / Playtime


## How to use
 1. Just open by `Tools/History/Open History` 
 2. I recommend setting up shortcuts between `Go Back` and `Go Forward` using the Unity Shortcut Menu:
    ![wizard](/Documentation~/shortcuts-settings.png)


## System Requirements
Unity 2018.4.0 or later versions


## How to install

<details>
<summary>Add from OpenUPM <em>| via scoped registry, recommended</em></summary>

This package is available on OpenUPM: https://openupm.com/packages/com.brunomikoski.editorhistorypanel

To add it the package to your project:

- open `Edit/Project Settings/Package Manager`
- add a new Scoped Registry:
  ```
  Name: OpenUPM
  URL:  https://package.openupm.com/
  Scope(s): com.brunomikoski
  ```
- click <kbd>Save</kbd>
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `com.brunomikoski.editorhistorypanel`
- click <kbd>Add</kbd>
</details>

<details>
<summary>Add from GitHub | <em>not recommended, no updates :( </em></summary>

You can also add it directly from GitHub on Unity 2019.4+. Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.

- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/brunomikoski/Animation-Sequencer.git`
- click <kbd>Add</kbd>
</details>
