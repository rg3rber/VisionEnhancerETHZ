
![VisionEnhancerLogo-crop](https://github.com/user-attachments/assets/ccc5f696-1225-4f1a-aacc-eefa075d4b2f)

## Overview

Vision Enhancer is a comprehensive Mixed Reality application for the Microsoft HoloLens 2 designed to support individuals with color blindness or visual impairments. Leveraging the power of Mixed Reality and Machine Learning, Vision Enhancer aims to break down visual barriers and empower users with tools that improve accessibility in education and beyond.

## Features

![architecture](https://github.com/user-attachments/assets/781d395e-0ca7-4a18-953b-347e10fc28e5)

### 1. Colorblindness Filters
Customizable filters for Protanopia, Deuteranopia, and Tritanopia.
Enables users to better distinguish colors and identify details, such as numbers in Ishihara test samples, that were previously not visible.

### 2. Magnification Tool
Magnify hard-to-see areas in real-time using the HoloLens 2 webcam.
For reading distant text, such as a blackboard or presentation slides.

### 3. Text-to-Speech Vision API
Employs cutting-edge vision model APIs to recognize and read out text.
Assists users with low vision or reading challenges by combining Mixed Reality and AI-powered accessibility.

![pipeline-img](https://github.com/user-attachments/assets/77a91553-e1cb-4af4-86bd-5dbf3dd10e8f)


## Demo Video



  ## How to Get Started

  ### Prerequisites

  - Unity 2022.3.47f1 LTS
  - For the OCR feature a valid Google vision API key is needed as well as an OpenAI API key
  - Flask needs to be installed and the script server.py running on your computer in the same network
  - the IP adress of the Flask Backend has to be known in order to set it once the app is running
    

  1. Clone the repository:

             git clone https://github.com/yourusername/visionenhancer.git

3. Open the project in Unity 2022.3 LTS
4. Deploy the application to your Microsoft HoloLens 2 device.
