using System;
using System.IO;
using GLFW;
using static OpenGL.Gl;

class Program
{
	const int WinWidth = 800;
	const int WinHeight = 600;

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

	public static unsafe void Main()
	{
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

		// Create vertex buffers
		Mesh mesh = new Mesh(
			new float[] {
				-1.0f, -1.0f, 0.0f,
				1.0f, -1.0f, 0.0f,
				-1.0f, 1.0f, 0.0f,

				1.0f, 1.0f, 0.0f,
				-1.0f, 1.0f, 0.0f,
				1.0f, -1.0f, 0.0f,
			},
			new float[] {
				0.0f, 0.0f,
				1.0f, 0.0f,
				0.0f, 1.0f,

				1.0f, 1.0f,
				0.0f, 1.0f,
				1.0f, 0.0f,
			}
		);

		// Create shader program
		Shader shader = new Shader(vertShaderGLSL, fragShaderGLSL);
		shader.Bind();
		shader.SetUniform("tex", 0);

		// Create texture
		Texture tex = new Texture();

		// Create boards
		const int cellSize = 2;
		CellAutomata ca = new CellAutomata(WinWidth / cellSize, WinHeight / cellSize);
		ca.Randomize();

		var boardImage = new uint[ca.Board.Length];

		// Main loop
		while (!Glfw.WindowShouldClose(win)) {
			// Space - Pause while held
			bool paused = Glfw.GetKey(win, Keys.Space) == InputState.Press;
			// R - Randomize board
			if (Glfw.GetKey(win, Keys.R) == InputState.Press)
				ca.Randomize();
			// C - Clear board
			if (Glfw.GetKey(win, Keys.C) == InputState.Press)
				ca.Clear();

			// LMB/RMB - Add/Remove cell
			Glfw.GetCursorPosition(win, out var mx, out var my);
			int mouseCellIdx = ca.IndexFromCoords((int)mx / cellSize, (int)my / cellSize);
			if (mouseCellIdx > 0 && mouseCellIdx < ca.Board.Length) {
				if (Glfw.GetMouseButton(win, MouseButton.Left) == InputState.Press)
					ca.Board[mouseCellIdx] = true;
				if (Glfw.GetMouseButton(win, MouseButton.Right) == InputState.Press)
					ca.Board[mouseCellIdx] = false;
			}

			if (!paused) {
				Thread.Sleep(10);
				ca.Tick();
			}

			// Generate image from current board
			for (uint i = 0; i < ca.Board.Length; ++i)
				boardImage[i] = ca.Board[i] ? 0xFFFFFFFF : 0xFF000000;

			// Draw generated image
			tex.Bind();
			tex.UploadRGBA8(ca.Width, ca.Height, boardImage);

			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			mesh.Draw();

			Glfw.SwapBuffers(win);
			Glfw.PollEvents();
		}
	}
}

