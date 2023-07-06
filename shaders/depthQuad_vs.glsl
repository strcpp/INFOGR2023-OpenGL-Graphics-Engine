﻿#version 330 core
layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vUV;

out vec2 TexCoords;

void main()
{
    TexCoords = vUV;
    gl_Position = vec4(vPosition, 1.0);
}