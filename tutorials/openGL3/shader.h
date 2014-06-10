#ifndef SHADER_H
#define SHADER_H

#include <string>
#include <GL/glew.h>

class Shader{
public:
	Shader(const std::string& fileName);

	//set the GPU in a state that uses vertex and fragment shaders that are defined here, 
	//instead of using none or another shader function
	void Bind();

	virtual ~Shader();
protected:
private:
	static const unsigned int NUM_SHADERS = 2; //One shader for fragment and one for vertex... if you use an additional geometry shader, you'd set it to 3
	Shader(const Shader& other){}
	std::string LoadShader(const std::string& fileName);
	void CheckShaderError(GLuint shader, GLuint flag, bool isProgram, const std::string& errorMessage);
	GLuint CreateShader(const std::string& text, unsigned int type);
	void operator=(const Shader& other){}

	GLuint mProgram; //Keeps track of where the program is; this is a handle
	GLuint mShaders[NUM_SHADERS];
};
#endif // SHADER_H