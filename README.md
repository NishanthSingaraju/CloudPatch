# CloudPatch

CloudPatch is a tool that streamlines the process of connecting your Unity Game to any CDN. This includes automating asset uploads, authentication, and setting configurations. Currently, we only support GCP, but support for Unstatiq, AWS, and PlayFab will be added soon (~1 week).

With CloudPatch, you have access to a menu where you can build and send your addressables to the cloud, input your API key to authenticate with your CDN, and point your CDN to the right game/bucket.

## Getting Started
These instructions will help you import the CloudPatch package into your Unity project using a Git URL.

1) In Unity, go to Assets > Import Package > Custom Package.
2) In the Import Unity Package window, click on the "Add Package from git URL..." button.
3) Enter the Git URL for the CloudPatch package (https://github.com/NishanthSingaraju/CloudPatch.git)
4) Click the "Add" button to import the package into your project.

## Features

CloudPatch streamlines the process of connecting your Unity Game to any CDN, including asset upload, authentication, and setting configurations.

### (1) Select a Service
- Choose between different backend providers i.e GCP, Unstatiq, GameKit, etc.

### (2) Set Settings
- Input the necessary parameters for your backend provider. For example, for GCP, input the bucket where you want to store your assets.

### (3) Authenticate
- Provide API key or service_account.json to authenticate with your CDN.
- API key used to upload assets 
- Embeds API key into your application for runtime access to the CDN.

Coming Soon: Connect CDN authentication with user authentication. Allow options such as Oauth 2.0 or Unstatiq Authentication.
### (4) Patch/Upload Directory/Build
- Patch: builds and sends your addressables to your CDN.
- Upload Directory: uploads an arbitrary folder to your CDN.
- Build: only builds your addressables.
- Coming Soon: Takes delta of addressables and uploads only a portion.


Note: Currently, only GCP is supported, but support for Unstatiq, AWS, and PlayFab will be added soon.

## Contributing
Please read CONTRIBUTING.md for details on our code of conduct, and the process for submitting pull requests to us.

License
This project is licensed under the Apache 2.0 License - see the LICENSE.md file for details.
