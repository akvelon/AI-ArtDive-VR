# Pano Tools

Set of the scripts and utilities to help working with AI-ArtDive VR

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
    
    > git clone --single-branch --branch dev https://github.com/akvelon/AI-ArtDive-Pano-Tools.git

6. Go to the project `pano-tools` directory created after `git clone`
    > cd pano-tools

7. Copy to the `pano-tools/gen` subdirectory one or more panoramas in EXR format (like `my_beautiful_panorama_8k.exr`) to process them <br />
    **Tip**: WSL2 filesystem can be accessed via File Explorer on path `\\wsl$\` (for example `\\wsl$\Ubuntu` for the Ubuntu distro)

8. Go to the `pano-tools/scripts` subdirectory of the project via `Windows Terminal` and run converting script:
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

9. Processed panoramas in `.jpg` format can be found in separate subdirectories of the `pano-tools/gen` project directory