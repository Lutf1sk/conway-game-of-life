using static OpenGL.Gl;

public class Texture
{
	private uint _texture;

	public Texture()
	{
		glActiveTexture(GL_TEXTURE0);
		_texture = glGenTexture();
		glBindTexture(GL_TEXTURE_2D, _texture);

		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_NEAREST);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	}

	~Texture()
	{
		glDeleteTexture(_texture);
	}

	public void Bind()
	{
		glActiveTexture(GL_TEXTURE0);
		glBindTexture(GL_TEXTURE_2D, _texture);
	}

	public unsafe void UploadRGBA8(int width, int height, uint[] pxData)
	{
		glActiveTexture(GL_TEXTURE0);
		glBindTexture(GL_TEXTURE_2D, _texture);

		fixed (uint* pxDataPtr = &pxData[0]) {
			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, pxDataPtr);
		}
		glGenerateMipmap(GL_TEXTURE_2D);
	}
}
