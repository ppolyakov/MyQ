# MyQ
# Cleaning Robot Console Application

The Cleaning Robot Console Application is a C# program that simulates an autonomous cleaning robot. The robot can navigate through a room, clean surfaces, and perform back-off strategies in case of obstacles or low battery.

## Table of Contents
1. [Introduction](#introduction)
2. [Features](#features)
3. [Requirements](#requirements)
4. [Usage](#usage)
    - [Building the Project](#building-the-project)
    - [Running the Application](#running-the-application)
    - [Unit Tests](#unit-tests)
5. [Input JSON Format](#input-json-format)
6. [Output JSON Format](#output-json-format)

## Introduction

The cleaning robot application is designed to demonstrate the capabilities of an autonomous cleaning robot. It receives a room map and a set of cleaning commands in JSON format, simulates the robot's movements, and generates an output JSON file with the final state of the robot.

## Features

- Reads room map and cleaning commands from an input JSON file.
- Simulates the robot's movements and cleaning actions based on the commands.
- Handles back-off strategies if the robot encounters obstacles or low battery.
- Logs the executed commands and back-off strategies to the console for monitoring.

## Usage

## Requirements

To run the Cleaning Robot Console Application, you need the following:

- .NET Core SDK (version 3.1 or later)
- C# development environment (e.g., Visual Studio, Visual Studio Code)

### Building the Project

1. Clone this repository to your local machine.
2. Open the solution file (`RobotCleaner.sln`) in your C# development environment.
3. Build the project to generate the executable file.

## Input JSON Format
The input JSON file should contain the following parameters:

map: A 2D array representing the room map with cells (S: cleanable space, C: column).
start: An object representing the initial position and facing direction of the robot (X: x-coordinate, Y: y-coordinate, Facing: "N", "W", "S", or "E").
commands: An array of strings representing the commands for the robot (e.g., ["TL", "A", "C", "A"]).
battery: An integer representing the initial battery level of the robot.

## Output JSON Format
The output JSON file will contain the following parameters:

Visited: An array of objects representing the unique positions visited by the robot during the cleaning process.
Cleaned: An array of objects representing the unique positions where the robot cleaned.
FinalPosition: An object representing the robot's final position and facing direction.
Battery: An integer representing the remaining battery after the cleaning process.


