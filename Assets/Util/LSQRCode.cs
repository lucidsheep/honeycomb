using ZXing;
using ZXing.QrCode;
using UnityEngine;

public class LSQRCode
{
    public enum ColorMode { BlackOnWhite, WhiteOnBlack, Custom }
    public static Color customCodeColor = Color.black;
    public static Color customBGColor = Color.white;
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }
    public static Sprite GenerateQRCode(string text, ColorMode mode = ColorMode.BlackOnWhite)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        if (mode != ColorMode.BlackOnWhite)
        {
            for (int i = 0; i < color32.Length; i++)
            {
                var pixel = color32[i];
                if (mode == ColorMode.WhiteOnBlack)
                    color32[i] = new Color32((byte)~pixel.r, (byte)~pixel.g, (byte)~pixel.b, pixel.a);
                else
                    color32[i] = pixel.r > 0 ? customBGColor : customCodeColor;
            }
        }
        encoded.SetPixels32(color32);
        encoded.Apply();
        return Sprite.Create(encoded, new Rect(0f, 0f, 256f, 256f),new Vector2(.5f, .5f));
    }
}