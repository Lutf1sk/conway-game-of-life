using static OpenGL.Gl;

public class Shader
{
	private uint _program;

	public Shader(string vertSource, string fragSource)
	{
		var vertShader = glCreateShader(GL_VERTEX_SHADER);
		glShaderSource(vertShader, vertSource);
		glCompileShader(vertShader);

		var fragShader = glCreateShader(GL_FRAGMENT_SHADER);
		glShaderSource(fragShader, fragSource);
		glCompileShader(fragShader);

		_program = glCreateProgram();
		glAttachShader(_program, vertShader);
		glAttachShader(_program, fragShader);

		glLinkProgram(_program);

		glDeleteShader(vertShader);
		glDeleteShader(fragShader);
	}

	~Shader()
	{
		glDeleteProgram(_program);
	}

	public void Bind()
	{
		glUseProgram(_program);
	}

	public void SetUniform(string key, int val)
	{
		var location = glGetUniformLocation(_program, key);
		glUniform1i(location, val);
	}
}
