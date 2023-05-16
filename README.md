# The idea of AI ArtDive VR
Our application uses AI to infuse panoramic photos with artistic styles, creating immersive environments that transport users to new worlds. We combined virtual reality technology with Stable Diffusion neural network to create AI ArtDive VR, which allows users to explore these environments in an immersive way. Our goal is to inspire and educate users about art's impact on our lives and push the boundaries of immersive experiences with AI and VR technology.
# How to setup and run project
## Installing the application in Unity
1. As a first step – we must download and install Unity Hub from https://unity.com/download
![img1](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-1.png)
2. After installation, on the first run, please login to the hub (registrer new account if needed), and skip editor installation step (we need exact version to be installed, so we will do it manually):
![img2](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-2.png)
3. Then, with hub opened in the background, go to https://unity.com/releases/editor/archive and select 2021.3.11f1 version of editor to install it through the Hub:
![img4](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-3.png)
4. Browser will switch to the Hub, and open installation dialogue. Please select “Android Build Support” to install:
![img5](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-4.png)
5. Then, we can clone repository with the project. Go to folder where you want to clone it and run “git clone git@github.com:akvelon/AI-ArtDive-VR.git” there.
6. Then, in Unity Hub, go to Projects – Open – add project from disk, and select subfolder “src/Deep Art VR Demo” inside cloned repository:
![img6](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-5.png)
It will be added in Projects list of the Hub.
Just click on this project and wait it loading (at first load – it will require some time to download and install dependencies and unroll project).
7. After opening – Editor can ask you to install new version – you can skip it – due to it does not required to run the project:
![img7](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-6.png)
8. Then – connect your Oculus Quest 2 to the PC and grant all requests in Oculus for this PC. Please make sure that you’ve enabled developer mode in your device.
In Unity3D editor go to File -> Build Settings, click on Android, and press “switch platform”:
![img8](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-7.png)
Wait for a while for scripts compilation.
9. Then, select your Oculus from “Run Device” drop-down list and press “Build And Run” (Unity will ask you how to name and where to place .apk file to build – please choose name and location you want on your PC):
![img8](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-8.png)

# Cutting and Stitching Panoramas
Set of the scripts and utilities to help working with AI-ArtDive VR. This tool is designed to assist you in processing 360 panoramas by dividing them into smaller parts. By breaking down the panoramas, you can apply the Stable Diffusion neural network to each section individually, enabling more efficient and accurate processing.

## deep-art-utils
AI-ArtDive utils to work with 2D images:
- DeepArtConsoleClient: a console client app for the batch 2D images converting process to apply AI-ArtDive effects.

## pano-converter

Scripts and utilities for converting 360 panorama to AI-ArtDive one.

Execution entry point of the `pano-converter` is a shell script `run-in-docker.sh`. It starts the execution in the docker environment, which consists from `Ubuntu 20.04 LTS` distro with preconfigured packages specified in the section `Apt packages`. 
In addition Dockerfile installs these utils:
- Blender
- Hugin (Nona, Enblend)
- DeepArtConsoleClient

Installation procedures for the utils above were specified in particular sections below.

After Docker file has prepared execution environment the convertion itself is started by Python script `convert.py`. The process of the convertion can be divided in 3 stages:

1. Process of the splitting an original 360 panorama image to bunch of 2D ones _using Blender_. Blender uses `camera_animation.py` python script and preliminary world scene `blender_panorama.blend` to start process of the convertion. An original 360 panorama is used as a background texture of the world scene in Blender. Bunch of 2D images are produced as a result of virtual camera shooting rotating in the center of the scene. It's a reverse process to the process of producing 360 panorama from 2D ones when camera is needed to be rotated in 360 degrees in different angles.
2. Process of the applying effects to the bunch of 2D images using `DeepArtConsoleClient` utility. The utlity converts each 2D image using AI-ArtDive cloud API.
3. Process of the sticking together bunch of 2D images what AI-ArtDive effects were applied to the resulting 360 panorama. It's performed by `Hugin` utilities: `Nona` and `Enblend` using pre-prepared project file `hugin_panorama.pto`. `Nona` performs preprocessing of the images. `Enblend` processes the splitting itself. Preprocessed splitting masks are used in the process of the splitting `enblend_ref-masks.tar.gz` to improve the resulting quality. Preprocessed splitting masks were processed on original without AI-ArtDive effect 2D images. It improves the quality because an original 2D image holds more information for the subsequent splitting process, after AI-ArtDive convertion details were lost.

### Usage

1. Install WSL2 for Windows 10/11
    - install in WSL2 latest stable Ubuntu distro:
    > wsl --install Ubuntu
    - also can choose another one to be installed from the list:
    > wsl --list --online

2. Install Docker for Windows using WSL2 environment
    - installation link: https://docs.docker.com/desktop/install/windows-install/
    - ensure in the installation wizard `Use WSL 2 based engine` checkbox is selected
    - more information about Docker installation process: https://docs.docker.com/desktop/windows/wsl/
        - note: `Enabling Docker support in WSL 2 distros` part is outdated and not needed in new Windows releases

3. Install `Windows Terminal` (https://apps.microsoft.com/store/detail/windows-terminal/9N0DX20HK701) via Windows Store to simplify working with utility

4. Run `Windows Terminal` and open tab with `Ubuntu` distro

5. Navigate to the working directory via `Windows Terminal` and `git clone` for `dev` branch
    
    > git clone --single-branch --branch dev https://github.com/akvelon/AI-ArtDive-VR.git

6. Go to the project `AI-ArtDive-VR/src/Pano Tools` directory created after `git clone`
    > cd "AI-ArtDive-VR/src/Pano Tools"

7. Copy to the `./gen` subdirectory one or more panoramas in EXR format (like `my_beautiful_panorama_8k.exr`) to process them <br />
    **Tip**: WSL2 filesystem can be accessed via File Explorer on path `\\wsl$\` (for example `\\wsl$\Ubuntu` for the Ubuntu distro)

8. Go to the `./scripts` subdirectory of the project via `Windows Terminal` and run converting script:
    > ./run-pano-converter.sh

        Script command line arguments

        1. Processing mode
        '-m', '--mode'
        Mode: split, convert, merge, all, all-without-convert
        default: all-without-convert
        usage example:
        > ./run-pano-converter.sh --mode all

        2. DeepArt effects to apply
        '-e','--effects'
        Effects: Comics, Candy, Twilight, Udnie, The Scream, VIII, Starry Night, Rain Princess, Watercolor, Pixel Art, Pen
        default: Comics Candy Twilight
        usage example:
        > ./run-pano-converter.sh --mode all --effects Comics Candy Twilight

9. Processed panoramas in `.jpg` format can be found in separate subdirectories of the `./gen` project directory


## Stylizing 3D Panoramas
1. Split the source 2D equirectangular image using Blender software (read the instructions above).
2. Apply the Stable Diffusion model to each projected image for style change (ensure you have the Stable Diffusion model, a deep learning model released in 2022, primarily used for image-to-image translation).
3. Condition the model on the source image and the desired style to generate a new image with the same content but a different artistic style.
4. Experiment with different styles and create a diverse set of stylized images.
5. Be aware that the Stable Diffusion model may introduce artifacts and deform objects (to address this, use a guide to control the stylization process).
6. Utilize the MiDaS neural network to convert projected images into depth maps.
7. During stylization, use a loss function based on the difference between the depth map of the stylized image and the depth of the original image.
8. This helps preserve the depth information and improves the final result.
9. Set the following parameters to generate a stylized image:
Prompt: _"an oil-on-canvas painting in the style of Starry Night, constellations, incredible detail, trending on artstation"_.
Negative prompt: _"ugly, tiling, poorly drawn hands, poorly drawn feet, poorly drawn face, out of frame, extra limbs, disfigured, deformed, body out of frame, bad anatomy, watermark, signature, cut off, low contrast, underexposed, overexposed, bad art, beginner, amateur, distorted face"_
* Steps: 50
* Sampler: DDIM
* CFG scale: 7
* Size: 1024x1024
* Denoising strength: 0.5
![img9](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-9.png)
10. Stylize the images using the specified settings.
11. Increase the resolution of the stylized images.
12. Use the R-ESRGAN 4x+ neural network to upscale the images up to 4096x4096 without losing quality.
13. Stitch the scaled stylized images together using pre-calculated anchor points and masks.
14. Save the resulting stitched image as an equirectangular 2D image.
15. Integrate the final image into the VR application for a seamless and immersive user experience.

## New VR scene adding

The simplest way to add new scene with pre-defined styles/joined scripts, etc. is to copy it from the set of already implemented scenes (**except _Lobby scene_**). Please assign newly copied scene appropriate name.

New scene must be included in scenes list in build settings:

![img10](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-10.png)
Then, go to _Lobby scene,_ and add (also, easiest way is to copy it from existed one) new element with toggle in _UI -> PanoEffects -> CylinderPointableCanvas -> Cylinder -> Canvas._ The name of this element **must be** the same, as of created of previous step scene.

Also, please check, that SwitchScenesController script is attached to the element:

![img11](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-11.png)
Please move the element to appropriate position on the map using green and red arrows on scene view.

## VR scene panorama changing

To change style of panorama in given scene, you should prepare sprite for preview picture and new styled panorama.

**For preview**, copy image in appropriate location (_Deep Art VR demo/Assets/PanoViewer/Art/Sprites/Panorama previews/_) with _Sprite (2D and UI)_ texture type:

![img12](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-12.png)

Then, assign it to corresponding image background (for example, _UI/PanoEffects/CylinderPointableCanvas/Cylinder/Pointable/CanvasMenu/Layout//Background -> Image -> Source Image):_

![img13](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-13.png)
**For panorama**, copy image to _Deep Art VR demo/Assets/PanoViewer/Art/Texture/Panoramas/_) with _Default_ texture type and _Cube_ texture shape:

![img14](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-14.png)

Then, assign new texture and preview in corresponding _Element_ in Panorama -> Pano List:

![img15](https://github.com/akvelon/AI-ArtDive-VR/blob/main/Screenshots/image-15.png)

# Additional information
here's a [link to an article about the AI ArtDive VR](https://akvelon.com/discover-art-in-a-new-dimension-with-ai-artdive-vr/) project.

If you are interested in experiencing the power of AI-generated immersive artistic environments, reach out to us at hello@akvelon.com to learn more about our application.
