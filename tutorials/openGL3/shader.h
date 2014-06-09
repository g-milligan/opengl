#ifndef SHADER_H
#define SHADER_H

#include <string>

class Shader{
public:
	Shader();

	virtual ~Shader();
protected:
private:
	Shader(const Shader& other){}
	Shader& operator=(const Shader& other){}
};
#endif // SHADER_H