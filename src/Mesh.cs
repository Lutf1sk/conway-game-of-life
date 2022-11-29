using static OpenGL.Gl;

public class Mesh
{
	uint _vao;
	uint _posBuffer, _uvBuffer;
	int _indexCount;

	public unsafe Mesh(float[] positions, float[] uvs)
	{
		_indexCount = positions.Length / 3;

		_vao = glGenVertexArray();
		glBindVertexArray(_vao);

		_posBuffer = glGenBuffer();
		glBindBuffer(GL_ARRAY_BUFFER, _posBuffer);
		fixed (float* posPtr = &positions[0]) {
			glBufferData(GL_ARRAY_BUFFER, sizeof(float) * positions.Length, posPtr, GL_STATIC_DRAW);
		}
		glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
		glEnableVertexAttribArray(0);

		_uvBuffer = glGenBuffer();
		glBindBuffer(GL_ARRAY_BUFFER, _uvBuffer);
		fixed (float* uvsPtr = &uvs[0]) {
			glBufferData(GL_ARRAY_BUFFER, sizeof(float) * uvs.Length, uvsPtr, GL_STATIC_DRAW);
		}
		glVertexAttribPointer(1, 2, GL_FLOAT, false, 2 * sizeof(float), NULL);
		glEnableVertexAttribArray(1);
	}

	~Mesh()
	{
		glDeleteBuffer(_posBuffer);
		glDeleteBuffer(_uvBuffer);
		glDeleteVertexArray(_vao);
	}

	public void Draw() {
		glBindVertexArray(_vao);
		glDrawArrays(GL_TRIANGLES, 0, _indexCount);
	}
}
