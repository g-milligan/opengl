#include "shader.h"
#include <fstream>
#include <iostream>

//constructor
Shader::Shader(const std::string& fileName){
	mProgram = glCreateProgram();

	//create the vertex and fragment shader (no standard file extensions... just using .vs and .fs)
	mShaders[0] = CreateShader(LoadShader(fileName+".vs"), GL_VERTEX_SHADER); //0 index
	mShaders[1] = CreateShader(LoadShader(fileName+".fs"), GL_FRAGMENT_SHADER); //1 index

	//add the shaders to the program
	for(unsigned int i = 0; i < NUM_SHADERS; i++){
		//adds one of the shaders to the GL program
		glAttachShader(mProgram, mShaders[i]);
	}

	//tells OpenGL what part of the program to read as what variable
	//bind 0 to "position" in the program (makes more sense with vertex shader part of the program)
	glBindAttribLocation(mProgram, 0, "position");

	//link the compiled shader code into the program
	glLinkProgram(mProgram);
	//check to see if the linking failed and display an error message if necessary
	CheckShaderError(mProgram, GL_LINK_STATUS, true, "Error: Program linking failed: ");

	glValidateProgram(mProgram);
	//check to see if the program is still valid after linking
	CheckShaderError(mProgram, GL_VALIDATE_STATUS, true, "Error: Program is invalid: ");
}

//destructor
Shader::~Shader(){
	for(unsigned int i = 0; i < NUM_SHADERS; i++){
		glDetachShader(mProgram, mShaders[i]);
		glDeleteShader(mShaders[i]);
	}
	glDeleteProgram(mProgram);
}

void Shader::Bind(){
	glUseProgram(mProgram);
}

GLuint Shader::CreateShader(const std::string& text, unsigned int type)
{
    GLuint shader = glCreateShader(type);

    if(shader == 0)
		std::cerr << "Error compiling shader type " << type << std::endl;

    const GLchar* p[1];
    p[0] = text.c_str();
    GLint lengths[1];
    lengths[0] = (GLint)text.length();

    glShaderSource(shader, 1, p, lengths);
    glCompileShader(shader);

    CheckShaderError(shader, GL_COMPILE_STATUS, false, "Error compiling shader!");

    return shader;
}

std::string Shader::LoadShader(const std::string& fileName)
{
    std::ifstream file;
    file.open((fileName).c_str());

    std::string output;
    std::string line;

    if(file.is_open())
    {
        while(file.good())
        {
            getline(file, line);
			output.append(line + "\n");
        }
    }
    else
    {
		std::cerr << "Unable to load shader: " << fileName << std::endl;
    }

    return output;
}
void Shader::CheckShaderError(GLuint shader, GLuint flag, bool isProgram, const std::string& errorMessage)
{
    GLint success = 0;
    GLchar error[1024] = { 0 };

    if(isProgram)
        glGetProgramiv(shader, flag, &success);
    else
        glGetShaderiv(shader, flag, &success);

    if(success == GL_FALSE)
    {
        if(isProgram)
            glGetProgramInfoLog(shader, sizeof(error), NULL, error);
        else
            glGetShaderInfoLog(shader, sizeof(error), NULL, error);

        std::cerr << errorMessage << ": '" << error << "'" << std::endl;
    }
}

