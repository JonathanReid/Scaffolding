Shader "Sprites/SubstractClipping"{
     Properties{
         _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
      }
 
     SubShader{
         
         Tags{"Queue" = "Transparent+1"}
 
         Pass{
             ZWrite on
             Offset -1, -1
             ColorMask 0
             Blend SrcAlpha OneMinusSrcAlpha
         }
     }
 }