# IssueTracker
The purpose of this project is to create, update and close issues on Github and Gitlab

## Functionalities

### 1. Adding new issue 
- **Endpoint:** `POST /issue/create` 
- **Description:** Adds new issue with the given name and description. 
- **Headers**
  - `accessToken` (string, <span style="color: yellow;">required</span>) - Access token generated on Github or GitLab
  - `gitHubUserName` (string) - Name of Github user. <span style="color: yellow;">Required when provider set to **Github**</span>
  - `gitHubRepository` (string) - Name of repository on Github where we add issue. <span style="color: yellow;">Required when provider set to **Github**</span>
  - `gitLabProjectId` (integer) - Id of GitLab project. <span style="color: yellow;">Required when provider set to **GitLab**</span>
- **Path parameters**
  - `provider` (string, <span style="color: yellow;">required</span>) - Determines where to send issue. Can be one of: `GitHub`, `GitLab`
- **Body parameters <span style="color: yellow;">required</span>** 
  - `title` (string) - Issue title
  - `description` (string) - Issue description 
  - *Sample Body:*
    ```json
    {
        "title": "Title of an Issue",
        "description": "Description of an Issue"
    }
  
### 2. Updating existing issue
- **Endpoint:** `POST /issue/update`
- **Description:** Update an issue with the given name and description.
- **Headers**
    - `accessToken` (string, <span style="color: yellow;">required</span>) - Access token generated on Github or GitLab
    - `gitHubUserName` (string) - Name of Github user. <span style="color: yellow;">Required when provider set to **Github**</span>
    - `gitHubRepository` (string) - Name of repository on Github where we add issue. <span style="color: yellow;">Required when provider set to **Github**</span>
    - `gitLabProjectId` (integer) - Id of GitLab project. <span style="color: yellow;">Required when provider set to **GitLab**</span>
- **Path parameters**
    - `provider` (string, <span style="color: yellow;">required</span>) - Determines where to send issue. Can be one of: `GitHub`, `GitLab`
    - `issueNumber` (integer <span style="color: yellow;">required</span>) - Updated issue id
- **Body parameters <span style="color: yellow;">required</span>**
    - `title` (string) - Issue title
    - `description` (string) - Issue description
    - *Sample Body:*
      ```json
      {
          "title": "Title of an Issue",
          "description": "Description of an Issue"
      }
### 3. Closing issue
- **Endpoint:** `POST /closeIssue`
- **Description:** Close an issue with provided id.
- **Headers**
    - `accessToken` (string, <span style="color: yellow;">required</span>) - Access token generated on Github or GitLab
    - `gitHubUserName` (string) - Name of Github user. <span style="color: yellow;">Required when provider set to **Github**</span>
    - `gitHubRepository` (string) - Name of repository on Github where we add issue. <span style="color: yellow;">Required when provider set to **Github**</span>
    - `gitLabProjectId` (integer) - Id of GitLab project. <span style="color: yellow;">Required when provider set to **GitLab**</span>
- **Path parameters**
    - `provider` (string, <span style="color: yellow;">required</span>) - Determines where to send issue. Can be one of: `GitHub`, `GitLab`
    - `issueNumber` (integer <span style="color: yellow;">required</span>) - Closed issue id
