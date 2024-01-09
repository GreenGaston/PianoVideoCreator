using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AcerolaColorGenerator 
{

    public static Color hsltoRGB(float h,float s, float l){
        h=h%1f;
        float r,g,b;
        if(s==0f){
            r=g=b=l;
        }
        else{
            float q=l<0.5f?l*(1f+s):l+s-l*s;
            float p=2f*l-q;
            r=HueToRGB(p,q,h+1f/3f);
            g=HueToRGB(p,q,h);
            b=HueToRGB(p,q,h-1f/3f);
        }
        //times 255 and cast to int
        // r=(int)(r*255f);
        // g=(int)(g*255f);
        // b=(int)(b*255f);
        return new Color(r,g,b);
    }

    public static float HueToRGB(float p,float q,float t){
        if(t<0f){
            t+=1f;
        }
        if(t>1f){
            t-=1f;
        }
        if(t<1f/6f){
            return p+(q-p)*6f*t;
        }
        if(t<1f/2f){
            return q;
        }
        if(t<2f/3f){
            return p+(q-p)*(2f/3f-t)*6f;
        }
        return p;
    }
    public static float[] hsvToRGB(float h,float s,float v){
        float r,g,b;
        float i=Mathf.Floor(h*6);
        float f=h*6-i;
        float p=v*(1-s);
        float q=v*(1-f*s);
        float t=v*(1-(1-f)*s);

        switch((int)i%6){
            case 0:
                r=v;
                g=t;
                b=p;
                break;
            case 1:
                r=q;
                g=v;
                b=p;
                break;
            case 2:
                r=p;
                g=v;
                b=t;
                break;
            case 3:
                r=p;
                g=q;
                b=v;
                break;
            case 4:
                r=t;
                g=p;
                b=v;
                break;
            case 5:
                r=v;
                g=p;
                b=q;
                break;
            //cant happen throw error
            default:
                r=0;
                g=0;
                b=0;
                break;
        }
        return new float[]{r,g,b};
    }

    public static float[] oklabTolinearsrgb(float L,float a,float b){
        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;

        float l=l_*l_*l_;
        float m=m_*m_*m_;
        float s=s_*s_*s_;

        float first=4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        float second=-1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        float third=-0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

        return new float[]{first,second,third};
    }

//     oklch_to_oklab(L, c, h) {
//   return [(L), (c * Math.cos(h)), (c * Math.sin(h))];
// }
    public static float[] oklchTooklab(float L,float c,float h){
        return new float[]{L,c*Mathf.Cos(h),c*Mathf.Sin(h)};
    }

    public static float Lerp(float min,float max,float t){
        return min+(max-min)*t;
    }

    public static float RandomRange(float min,float max){
        return UnityEngine.Random.value*(max-min)+min;
    }

   
    public static List<Color> GenerateHSL(string HueMode,PaletteSettings settings){
        List<Color> colors=new List<Color>();

        float hueBase=settings.HueBase;
        float hueContrast=Lerp(0.33f,1.0f,settings.HueContrast);

        float saturationBase=Lerp(0.01f,0.5f,settings.SaturationBase);
        float saturationContrast=Lerp(0.1f,1-saturationBase,settings.SaturationContrast);
        float saturationFixed=Lerp(0.1f,1.0f,settings.Fixed);

        float lightnessBase=Lerp(0.01f,0.5f,settings.LuminanceBase);
        float lightnessContrast=Lerp(0.1f,1-lightnessBase,settings.LuminanceContrast);
        float lightnessFixed=Lerp(0.3f,0.85f,settings.Fixed);

        bool saturationConstant=settings.SaturationConstant;
        bool lightnessConstant=!saturationConstant;

        if(HueMode=="Monochromatic"){
            saturationConstant=false;
            lightnessConstant=false;
        }

        for(int i=0;i<settings.ColorCount;++i){
            float linearIterator=(float)i/(settings.ColorCount-1);

            float hueOffset=linearIterator*hueContrast;

            if(HueMode=="Monochromatic") hueOffset*=0.0f;
            if(HueMode=="Analagous") hueOffset*=0.25f;
            if(HueMode=="Complementary") hueOffset*=0.33f;
            if(HueMode=="Triadic Complementary") hueOffset*=0.66f;
            if(HueMode=="Tetradic Complementary") hueOffset*=0.75f;

            if(HueMode!="Monochromatic"){
                hueOffset+=(UnityEngine.Random.value*2-1)*0.01f;
            }

            float saturation=saturationBase+linearIterator*saturationContrast;
            float lightness=lightnessBase+linearIterator*lightnessContrast;

            if(saturationConstant) saturation=saturationFixed;
            if(lightnessConstant) lightness=lightnessFixed;

            colors.Add(hsltoRGB(hueBase+hueOffset,saturation,lightness));
        }
        return colors;
    }

    public static List<Color> GenerateHSV(string HueMode,PaletteSettings settings){
        List<Color> colors=new List<Color>();

        float hueBase=settings.HueBase;
        float hueContrast=Lerp(0.33f,1.0f,settings.HueContrast);

        float saturationBase=Lerp(0.01f,0.5f,settings.SaturationBase);
        float saturationContrast=Lerp(0.1f,1-saturationBase,settings.SaturationContrast);
        float saturationFixed=Lerp(0.1f,1.0f,settings.Fixed);

        float valueBase=Lerp(0.01f,0.5f,settings.LuminanceBase);
        float valueContrast=Lerp(0.1f,1-valueBase,settings.LuminanceContrast);
        float valueFixed=Lerp(0.3f,1.0f,settings.Fixed);

        bool saturationConstant=settings.SaturationConstant;
        bool valueConstant=!saturationConstant;

        if(HueMode=="Monochromatic"){
            saturationConstant=false;
            valueConstant=false;
        }

        for(int i=0;i<settings.ColorCount;++i){
            float linearIterator=(float)i/(settings.ColorCount-1);

            float hueOffset=linearIterator*hueContrast;

            if(HueMode=="Monochromatic") hueOffset*=0.0f;
            if(HueMode=="Analagous") hueOffset*=0.25f;
            if(HueMode=="Complementary") hueOffset*=0.33f;
            if(HueMode=="Triadic Complementary") hueOffset*=0.66f;
            if(HueMode=="Tetradic Complementary") hueOffset*=0.75f;

            if(HueMode!="Monochromatic"){
                hueOffset+=(UnityEngine.Random.value*2-1)*0.01f;
            }

            float saturation=saturationBase+linearIterator*saturationContrast;
            float value=valueBase+linearIterator*valueContrast;

            if(saturationConstant) saturation=saturationFixed;
            if(valueConstant) value=valueFixed;

            float[] rgb=hsvToRGB(hueBase+hueOffset,saturation,value);
            colors.Add(new Color(rgb[0],rgb[1],rgb[2]));
        }
        return colors;
    }




    public static List<Color> generateOKLCH(string HueMode,PaletteSettings settings){
        List<Color> colors=new List<Color>();

        float hueBase=settings.HueBase*2*Mathf.PI;
        float hueContrast=Lerp(0.33f,1.0f,settings.HueContrast);

        float chromaBase=Lerp(0.01f,0.1f,settings.SaturationBase);
        float chromaContrast=Lerp(0.075f,0.125f-chromaBase,settings.SaturationContrast);
        float chromaFixed=Lerp(0.01f,0.125f,settings.Fixed);

        float lightnessBase=Lerp(0.3f,0.6f,settings.LuminanceBase);
        float lightnessContrast=Lerp(0.3f,1.0f-lightnessBase,settings.LuminanceContrast);
        float lightnessFixed=Lerp(0.6f,0.9f,settings.Fixed);

        bool chromaConstant=settings.SaturationConstant;
        bool lightnessConstant=!chromaConstant;

        if(HueMode=="Monochromatic"){
            chromaConstant=false;
            lightnessConstant=false;
        }

        for(int i=0;i<settings.ColorCount;++i){
            float linearIterator=(float)i/(settings.ColorCount-1);

            float hueOffset=linearIterator*hueContrast*2*Mathf.PI+(Mathf.PI/4);

            if(HueMode=="Monochromatic") hueOffset*=0.0f;
            if(HueMode=="Analagous") hueOffset*=0.25f;
            if(HueMode=="Complementary") hueOffset*=0.33f;
            if(HueMode=="Triadic Complementary") hueOffset*=0.66f;
            if(HueMode=="Tetradic Complementary") hueOffset*=0.75f;

            if(HueMode!="Monochromatic"){
                hueOffset+=(UnityEngine.Random.value*2-1)*0.01f;
            }

            float chroma=chromaBase+linearIterator*chromaContrast;
            float lightness=lightnessBase+linearIterator*lightnessContrast;

            if(chromaConstant) chroma=chromaFixed;
            if(lightnessConstant) lightness=lightnessFixed;

            float[] lab=oklchTooklab(lightness,chroma,hueBase+hueOffset);
            Debug.Log("lab:"+lab[0]+" "+lab[1]+" "+lab[2]);
            float[] rgb=oklabTolinearsrgb(lab[0],lab[1],lab[2]);

            rgb[0]=Mathf.Round(Mathf.Max(0.0f,Mathf.Min(rgb[0],1.0f)));
            rgb[1]=Mathf.Round(Mathf.Max(0.0f,Mathf.Min(rgb[1],1.0f)));
            rgb[2]=Mathf.Round(Mathf.Max(0.0f,Mathf.Min(rgb[2],1.0f)));

            colors.Add(new Color(rgb[0],rgb[1],rgb[2]));
        }
        return colors;
    }



    public class PaletteSettings{

        public float HueBase;
        public float HueContrast;
        public float SaturationBase;
        public float SaturationContrast;
        public float LuminanceBase;
        public float LuminanceContrast;
        public float Fixed;
        public bool SaturationConstant;
        public int ColorCount;

   
        public  PaletteSettings(int ColorCount){
            this.ColorCount=ColorCount;
            HueBase=UnityEngine.Random.value;
            HueContrast=UnityEngine.Random.value;
            SaturationBase=UnityEngine.Random.value;
            SaturationContrast=UnityEngine.Random.value;
            LuminanceBase=UnityEngine.Random.value*0.8f+0.2f;
            LuminanceContrast=UnityEngine.Random.value;
            Fixed=UnityEngine.Random.value;
            SaturationConstant=true;

        }
    }


    public static List<List<Color>> Generate(string Huemode,int ColorCount){
        List<List<Color>> colors=new List<List<Color>>();
        PaletteSettings settings=new PaletteSettings(ColorCount);
        colors.Add(GenerateHSL(Huemode,settings));
        colors.Add(GenerateHSV(Huemode,settings));
        colors.Add(generateOKLCH(Huemode,settings));
        return colors;
    }
}
