using System;
using System.IO;
using GLFW;
using static OpenGL.Gl;

class Program {
	const int WinWidth = 1200;
	const int WinHeight = 1000;

	const string vertShaderGLSL = @"
		#version 330

		layout(location = 0) in vec3 in_pos;
		layout(location = 1) in vec2 in_uv;

		out vec2 frag_uv;

		void main() {
			gl_Position = vec4(in_pos, 1.0);
			frag_uv = vec2(in_uv.x, 1.0 - in_uv.y);
		}
	";

	const string fragShaderGLSL = @"
		#version 330

		in vec2 frag_uv;

		out vec4 out_color;

		uniform sampler2D tex;

		void main() {
			out_color = texture(tex, frag_uv);
		}
	";

	public static unsafe void Main() {
		// Initialize window and GL context
		Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
		Glfw.WindowHint(Hint.ContextVersionMajor, 3);
		Glfw.WindowHint(Hint.ContextVersionMinor, 3);
		Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
		Glfw.WindowHint(Hint.Doublebuffer, true);
		Glfw.WindowHint(Hint.Decorated, true);
		Glfw.WindowHint(Hint.Resizable, false);
		var win = Glfw.CreateWindow(WinWidth, WinHeight, "Skjut mig", GLFW.Monitor.None, Window.None);

		Glfw.MakeContextCurrent(win);
		Import(Glfw.GetProcAddress);

		glEnable(GL_CULL_FACE);
		glCullFace(GL_BACK);
		glEnable(GL_DEPTH_TEST);

		// Create vertex buffers
		var vao = glGenVertexArray();
		glBindVertexArray(vao);

		var verts = new float[] {
			-1.0f, -1.0f, 0.0f,
			1.0f, -1.0f, 0.0f,
			-1.0f, 1.0f, 0.0f,

			1.0f, 1.0f, 0.0f,
			-1.0f, 1.0f, 0.0f,
			1.0f, -1.0f, 0.0f,
		};

		var uvs = new float[] {
			0.0f, 0.0f,
			1.0f, 0.0f,
			0.0f, 1.0f,

			1.0f, 1.0f,
			0.0f, 1.0f,
			1.0f, 0.0f,
		};

		var vbo = glGenBuffer();
		glBindBuffer(GL_ARRAY_BUFFER, vbo);
		fixed (float* vertsPtr = &verts[0]) {
			glBufferData(GL_ARRAY_BUFFER, sizeof(float) * verts.Length, vertsPtr, GL_STATIC_DRAW);
		}
		glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
		glEnableVertexAttribArray(0);

		vbo = glGenBuffer();
		glBindBuffer(GL_ARRAY_BUFFER, vbo);
		fixed (float* uvsPtr = &uvs[0]) {
			glBufferData(GL_ARRAY_BUFFER, sizeof(float) * verts.Length, uvsPtr, GL_STATIC_DRAW);
		}
		glVertexAttribPointer(1, 2, GL_FLOAT, false, 2 * sizeof(float), NULL);
		glEnableVertexAttribArray(1);

		// Create shader program
		var vertShader = glCreateShader(GL_VERTEX_SHADER);
		glShaderSource(vertShader, vertShaderGLSL);
		glCompileShader(vertShader);

		var fragShader = glCreateShader(GL_FRAGMENT_SHADER);
		glShaderSource(fragShader, fragShaderGLSL);
		glCompileShader(fragShader);

		var shaderProgram = glCreateProgram();
		glAttachShader(shaderProgram, vertShader);
		glAttachShader(shaderProgram, fragShader);

		glLinkProgram(shaderProgram);

		glDeleteShader(vertShader);
		glDeleteShader(fragShader);

		glUseProgram(shaderProgram);

		var texLocation = glGetUniformLocation(shaderProgram, "tex");
		glUniform1i(texLocation, 0);

		// Create texture
		glActiveTexture(GL_TEXTURE0);
		uint tex = 0;
		glGenTextures(1, &tex);
		glBindTexture(GL_TEXTURE, tex);

		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_NEAREST);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);


		// Create boards
		const int cellSize = 4;
		const int hCells = WinWidth / cellSize;
		const int vCells = WinHeight / cellSize;
		const int boardCells = hCells * vCells;

		var boards = new bool[][] {
			new bool[boardCells], new bool[boardCells]
		};

		var boardImage = new uint[boardCells];

		int boardIdx = 0;

		// Randomize initial board state
		var rand = new Random();
		for (uint i = 0; i < boardCells; ++i)
			boards[boardIdx][i] = rand.Next(3) == 0;

		// Main loop
		while (!Glfw.WindowShouldClose(win)) {
			// Space - Pause while held
			bool paused = Glfw.GetKey(win, Keys.Space) == InputState.Press;

			// R - Randomize board
			if (Glfw.GetKey(win, Keys.R) == InputState.Press)
				for (uint i = 0; i < boardCells; ++i)
					boards[boardIdx][i] = rand.Next(3) == 0;

			// C - Clear board
			if (Glfw.GetKey(win, Keys.C) == InputState.Press)
				for (uint i = 0; i < boardCells; ++i)
					boards[boardIdx][i] = false;

			// LMB/RMB - Add/Remove cell
			Glfw.GetCursorPosition(win, out var mx, out var my);
			int mouseCellX = (int)mx / cellSize;
			int mouseCellY = (int)my / cellSize;
			int mouseCellIdx = mouseCellY * hCells + mouseCellX;

			if (mouseCellIdx > 0 && mouseCellIdx < boardCells) {
				if (Glfw.GetMouseButton(win, MouseButton.Left) == InputState.Press)
					boards[boardIdx][mouseCellIdx] = true;
				if (Glfw.GetMouseButton(win, MouseButton.Right) == InputState.Press)
					boards[boardIdx][mouseCellIdx] = false;
			}

			// Update board
			if (!paused) {
// 				Thread.Sleep(60);

				int nextBoardIdx = (boardIdx + 1) % boards.Length;
				for (uint i = 0; i < boardCells; ++i) {
					int neighbours = 0;
					uint x = i % hCells;
					uint y = i / hCells;

					if (x > 0 && boards[boardIdx][i - 1])
						++neighbours;
					if (x < hCells-1 && boards[boardIdx][i + 1])
						++neighbours;
					if (y > 0 && boards[boardIdx][i - hCells])
						++neighbours;
					if (y < vCells-1 && boards[boardIdx][i + hCells])
						++neighbours;

					if (x > 0 && y > 0 && boards[boardIdx][i - hCells - 1])
						++neighbours;
					if (x < hCells-1 && y > 0 && boards[boardIdx][i - hCells + 1])
						++neighbours;
					if (x > 0 && y < vCells-1 && boards[boardIdx][i + hCells - 1])
						++neighbours;
					if (x < hCells-1 && y < vCells-1 && boards[boardIdx][i + hCells + 1])
						++neighbours;

					if (boards[boardIdx][i])
						boards[nextBoardIdx][i] = neighbours == 2 || neighbours == 3;
					else
						boards[nextBoardIdx][i] = neighbours == 3;
				}
				boardIdx = nextBoardIdx;
			}

			// Generate image from current board (could definitely be a lot faster)
			for (uint i = 0; i < boardCells; ++i)
				boardImage[i] = boards[boardIdx][i] ? 0xFFFFFFFF : 0xFF000000;
			fixed (uint* boardImagePtr = &boardImage[0]) {
				glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, hCells, vCells, 0, GL_RGBA, GL_UNSIGNED_BYTE, boardImagePtr);
			}
			glGenerateMipmap(GL_TEXTURE_2D);

			// Draw generated image
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glDrawArrays(GL_TRIANGLES, 0, verts.Length);

			Glfw.SwapBuffers(win);
			Glfw.PollEvents();
		}
	}
}

