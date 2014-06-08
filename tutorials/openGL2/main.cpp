#include <iostream>
#include <GL/glew.h>
#include "display.h"

int main(int argc, char** argv){
	Display display(800, 600, "Hello World!");
	//while the display is NOT closed
	while(!display.IsClosed()){
		//set a color and clear the buffer bit
		glClearColor(0.0f, 0.15f, 0.3f, 1.0f);
		glClear(GL_COLOR_BUFFER_BIT);
		//show updates by swapping buffers
		display.Update();
	}
	return 0;
}