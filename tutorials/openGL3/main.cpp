#include <iostream>
#include <GL/glew.h>
#include "display.h"
#include "shader.h"

#undef main
int main(int argc, char** argv){
	Display display(800, 600, "Hello World!");

	Shader shader("./res/basicShader");

	//while the display is NOT closed
	while(!display.IsClosed()){

		//set a color and clear the buffer bit
		glClearColor(0.0f, 0.15f, 0.3f, 1.0f);
		glClear(GL_COLOR_BUFFER_BIT);

		//vertex and fragment shaders
		shader.Bind();

		//show updates by swapping buffers
		display.Update();
	}
	return 0;
}