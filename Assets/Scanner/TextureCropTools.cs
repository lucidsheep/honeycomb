using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class TextureCropTools
{
    // Based on https://gist.github.com/natsupy/e129936543f9b4663a37ea0762172b3b

    public static Texture2D CropToSquare(Texture2D tex)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        var xOffset = (tex.width - smaller) / 2;
        var yOffset = (tex.height - smaller) / 2;
        return CropWithRect(tex, new Rect(xOffset, yOffset, smaller, smaller));
    }

    public static Texture2D CropToSquare(WebCamTexture tex)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        return CropWithRect(tex, new Rect(0, 0, smaller, smaller));
    }

    public static void CropToSquare(WebCamTexture tex, ref Texture2D outputTexture)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        CropWithRect(tex, ref outputTexture, new Rect(0, 0, smaller, smaller));
    }

    public static void CropToSquare(Texture2D tex, ref Texture2D outputTexture)
    {
        var smaller = tex.width < tex.height ? tex.width : tex.height;
        CropWithRect(tex, ref outputTexture, new Rect(0, 0, smaller, smaller));
    }

    public static Texture2D CropWithRect(Texture2D texture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        Texture2D result = new Texture2D((int)rect.width, (int)rect.height);

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = rect.x;
            float yRect = rect.y;
            float widthRect = rect.width;
            float heightRect = rect.height;

            xRect = (texture.width - rect.width) / 2;
            yRect = (texture.height - rect.height) / 2;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }

            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }

        Profiler.EndSample();
        return result;
    }

    public static void CropWithPctRect(Texture texture, Rect rect, RenderTexture result)
    {
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        if (texture == null || result == null || texture.height < result.height || texture.width < result.width)
            return;

            Graphics.CopyTexture(texture, 0, 0,
                Mathf.FloorToInt(texture.width * rect.x), Mathf.FloorToInt(texture.height * rect.y),
                result.width, result.height,
                result, 0, 0, 0, 0);

    }

    public static Texture2D CropWithRect(WebCamTexture texture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        Texture2D result = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = rect.x;
            float yRect = rect.y;
            float widthRect = rect.width;
            float heightRect = rect.height;

            xRect = (texture.width - rect.width) / 2;
            yRect = (texture.height - rect.height) / 2;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }
            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }

        Profiler.EndSample();
        return result;
    }

    public static void CropWithRect(WebCamTexture texture, ref Texture2D outputTexture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        if (outputTexture == null)
            outputTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        if (outputTexture.width != rect.width || outputTexture.height != rect.height)
            outputTexture.Reinitialize((int)rect.width, (int)rect.height);

        Texture2D result = outputTexture;

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = rect.x;
            float yRect = rect.y;
            float widthRect = rect.width;
            float heightRect = rect.height;

            xRect = (texture.width - rect.width) / 2;
            yRect = (texture.height - rect.height) / 2;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }
            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }
        Profiler.EndSample();
    }

    public static void CropWithRect(Texture2D texture, ref Texture2D outputTexture, Rect rect)
    {
        Profiler.BeginSample("TextureCropTools.CropWithRect");
        if (rect.height < 0 || rect.width < 0)
        {
            throw new System.ArgumentException("Invalid texture size");
        }

        if (outputTexture == null)
            outputTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

        if (outputTexture.width != rect.width || outputTexture.height != rect.height)
            outputTexture.Reinitialize((int)rect.width, (int)rect.height);

        Texture2D result = outputTexture;

        if (rect.width != 0 && rect.height != 0)
        {
            float xRect = rect.x;
            float yRect = rect.y;
            float widthRect = rect.width;
            float heightRect = rect.height;

            xRect = (texture.width - rect.width) / 2;
            yRect = (texture.height - rect.height) / 2;

            if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
                xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
                xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
            {
                throw new System.ArgumentException("Set value crop less than origin texture size");
            }
            result.SetPixels(texture.GetPixels(Mathf.FloorToInt(xRect), Mathf.FloorToInt(yRect),
                                            Mathf.FloorToInt(widthRect), Mathf.FloorToInt(heightRect)));
            result.Apply();
        }
        Profiler.EndSample();
    }


    static RenderTexture scaledRT;
    public static Texture SquareAndScaleTexture(Texture input, int size)
    {
        //scale
        bool heightLarger = input.height > input.width;
        int largerDim = heightLarger ? input.height : input.width;
        float scale = size / (float)largerDim;
        int scaledWidth = heightLarger ? Mathf.FloorToInt((float)input.width * scale) : size;
        int scaledHeight = heightLarger ? size : Mathf.FloorToInt((float)input.height * scale);
        //only reinit if dimensions have changed
        if (scaledRT == null || scaledRT.width != scaledWidth || scaledRT.height != scaledHeight)
        {
            if (scaledRT != null)
                scaledRT.Release();
            scaledRT = new RenderTexture(scaledWidth, scaledHeight, 24);
        }
        Graphics.Blit(input, scaledRT);

        //render to square
        Texture2D output = new Texture2D(size, size, TextureFormat.RGB24, false);
        RenderTexture.active = scaledRT;
        int xRect = heightLarger ? (size - scaledRT.width) / 2 : 0;
        int yRect = heightLarger ? 0 : (size - scaledRT.height) / 2;
        
        output.ReadPixels(new Rect(0, 0, scaledRT.width, scaledRT.height), xRect, yRect);
        output.Apply();
        RenderTexture.active = null;
        return output;
    }

    public static void SquareAndScaleToRenderTexture(Texture input, RenderTexture output)
    {
        int size = output.width;
        //scale
        bool heightLarger = input.height > input.width;
        int largerDim = heightLarger ? input.height : input.width;
        Vector2 scale = new Vector2(
             heightLarger ? (float)input.width / (float)size : 1f
            , heightLarger ? 1f : (float)input.height / (float)size);
        Graphics.Blit(input, output, scale, Vector2.zero);
    }

    public static Texture2D BufferToSquare(Texture texture)
    {
        Profiler.BeginSample("TextureCropTools.BuferToSquare");

        /*
        if(texture.width == texture.height)
        {
            Debug.Log("Texture is already a square");
            outputTexture = texture;
            return;
        }
        */
        bool heightLarger = texture.height > texture.width;
        int largerDim = heightLarger ? texture.height : texture.width;

        var outputTexture = new Texture2D(largerDim, largerDim, TextureFormat.RGB24, false);

        int xRect = heightLarger ? (largerDim / 2) - (texture.width / 2) : 0;
        int yRect = heightLarger ? 0 : (largerDim / 2) - (texture.height / 2);
        int widthRect = heightLarger ? texture.height / 2 : largerDim;
        int heightRect = heightLarger ? largerDim : texture.width / 2;

        /*
        if (texture.width < rect.x + rect.width || texture.height < rect.y + rect.height ||
            xRect > rect.x + texture.width || yRect > rect.y + texture.height ||
            xRect < 0 || yRect < 0 || rect.width < 0 || rect.height < 0)
        {
            throw new System.ArgumentException("Set value crop less than origin texture size");
        }
        */
        //  tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = texture as RenderTexture;
        Debug.Log("rt size " + RenderTexture.active.height);
        var rect = new Rect(0, 0, texture.width, texture.height);
        Debug.Log("rect " + rect.ToString() + " at " + xRect + "," + yRect);
        outputTexture.ReadPixels(rect, xRect, yRect);
        outputTexture.Apply();
        RenderTexture.active = null;
        Profiler.EndSample();
        return outputTexture;
    }

}