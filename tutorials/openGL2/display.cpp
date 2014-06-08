#include "display.h"
#include <GL/glew.h>
#include <iostream>

//constructor
Display::Display(int width, int height, const std::string& title){
	SDL_Init(SDL_INIT_EVERYTHING);

	SDL_GL_SetAttribute(SDL_GL_RED_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
	SDL_GL_SetAttribute(SDL_GL_BUFFER_SIZE, 32);
	SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER, 1); //allocate space for two screens that alternate

	mWindow=SDL_CreateWindow(title.c_str(), SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, width, height, SDL_WINDOW_OPENGL);
	//SDL makes it easy to talk to the graphics driver... create a SDL context for the window
	mGlContext=SDL_GL_CreateContext(mWindow);

	//show error if GLEW fails to init and check to see what openGL functions are available
	GLenum status=glewInit();
	if(status!=GLEW_OK){
		std::cerr << "Glew failed to initialize" << std::endl;
	}

	//window NOT closed on init
	mIsClosed=false;
}

//destructor
Display::~Display(){
	//objects deleted in the reverse order in which they were created... otherwise, "weird allocation bugs" may occur
	SDL_GL_DeleteContext(mGlContext);
	SDL_DestroyWindow(mWindow);
	SDL_Quit();
}

//method
bool Display::IsClosed(){return mIsClosed;}

//method
void Display::Update(){
	//use the double buffering... alternate to the other buffered window
	SDL_GL_SwapWindow(mWindow);
	//listen for operating system events
	SDL_Event e;
	while(SDL_PollEvent(&e)){
		//if operating system quit event type was triggered
		if(e.type==SDL_QUIT){
			mIsClosed=true;
		}
	}
}