internal class Shader
{
	public byte[][] texture;
	public bool HasTexture;
	public string shader;
	public Shader(byte[][] texture)
	{
		this.texture = texture;
		HasTexture = true;
		shader = "░▒▓█";
	}
	public Shader(string shader)
	{
		HasTexture = false;
		this.shader = shader;
	}
}

