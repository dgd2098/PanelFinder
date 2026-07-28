Shader "Hidden/Custom/MyNewEdgeDetectionPPS_V2" // 쉐이더 이름 변경
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (0,0,0,1)
        _NormalThreshold("Normal Threshold", Range(0, 1)) = 0.5
        _EdgeWidth("Edge Width", Range(0.001, 0.01)) = 0.002
    }
        SubShader
        {
            Cull Off ZWrite Off ZTest Always

            Pass
            {
                CGPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma target 3.0

                // --- 필요한 변수 선언 ---
                sampler2D _MainTex; // 이전 패스 결과
                sampler2D _CameraDepthTexture; // 카메라의 깊이 텍스처
                sampler2D _CameraNormalsTexture; // 카메라의 노멀 텍스처 (Depth Texture Mode: DepthNormals 시 생성)

                float4 _MainTex_TexelSize; // _MainTex의 텍셀 크기 (샘플링 오프셋 계산용)

                // 노멀 디코딩 함수 (UnityCG.cginc에서 가져온 것, 문제가 없다면 사용)
                // DepthNormals 텍스처는 (RG)에 노멀, (BA)에 깊이가 인코딩됨
                // 하지만 _CameraNormalsTexture는 이미 디코딩된 노멀을 제공할 수 있음.
                // 여기서는 _CameraNormalsTexture가 뷰 공간 노멀을 바로 준다고 가정하고,
                // _CameraDepthNormalsTexture의 DecodeViewNormalStereo 함수만 그대로 사용.
                float3 DecodeViewNormalStereo(float4 enc)
                {
                    float kScale = 1.7777;
                    float3 n;
                    n.xy = enc.xy * 2.0 - 1.0;
                    n.z = sqrt(1.0 - saturate(dot(n.xy, n.xy)));
                    n.xy *= kScale;
                    return n;
                }

                fixed4 _EdgeColor;
                float _NormalThreshold;
                float _EdgeWidth;

                struct VertInput {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct VertOutput {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                VertOutput Vert(VertInput v) {
                    VertOutput o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 Frag(VertOutput i) : SV_Target {
                    fixed4 originalColor = tex2D(_MainTex, i.uv);

                // 현재 픽셀의 노멀 값 가져오기
                // _CameraNormalsTexture를 직접 사용합니다.
                // 만약 이 텍스처가 존재한다면 바로 뷰 공간 노멀을 제공합니다.
                float3 centerNormal = tex2D(_CameraNormalsTexture, i.uv).rgb * 2.0 - 1.0; // 0~1 범위의 노멀을 -1~1로 변환

                // 혹은 _CameraDepthNormalsTexture를 사용하여 다시 시도 (가장 최신 시도한 방식)
                // float4 normalDepthPacked = tex2D(_CameraDepthNormalsTexture, i.uv);
                // float3 centerNormal = DecodeViewNormalStereo(normalDepthPacked);

                // 주변 픽셀 샘플링을 위한 텍셀 크기 (MainTex의 텍셀 사이즈 사용)
                float2 texelSize = _MainTex_TexelSize.xy;

                // 주변 픽셀의 노멀 값 샘플링 (8방향)
                // 여기도 _CameraNormalsTexture를 사용합니다.
                float3 normal_up = tex2D(_CameraNormalsTexture, i.uv + float2(0, _EdgeWidth) * texelSize).rgb * 2.0 - 1.0;
                float3 normal_down = tex2D(_CameraNormalsTexture, i.uv + float2(0, -_EdgeWidth) * texelSize).rgb * 2.0 - 1.0;
                float3 normal_left = tex2D(_CameraNormalsTexture, i.uv + float2(-_EdgeWidth, 0) * texelSize).rgb * 2.0 - 1.0;
                float3 normal_right = tex2D(_CameraNormalsTexture, i.uv + float2(_EdgeWidth, 0) * texelSize).rgb * 2.0 - 1.0;

                float3 normal_ur = tex2D(_CameraNormalsTexture, i.uv + float2(_EdgeWidth, _EdgeWidth) * texelSize).rgb * 2.0 - 1.0;
                float3 normal_ul = tex2D(_CameraNormalsTexture, i.uv + float2(-_EdgeWidth, _EdgeWidth) * texelSize).rgb * 2.0 - 1.0;
                float3 normal_dr = tex2D(_CameraNormalsTexture, i.uv + float2(_EdgeWidth, -_EdgeWidth) * texelSize).rgb * 2.0 - 1.0;
                float3 normal_dl = tex2D(_CameraNormalsTexture, i.uv + float2(-_EdgeWidth, -_EdgeWidth) * texelSize).rgb * 2.0 - 1.0;


                float normalDiff = 0;
                normalDiff += distance(centerNormal, normal_up);
                normalDiff += distance(centerNormal, normal_down);
                normalDiff += distance(centerNormal, normal_left);
                normalDiff += distance(centerNormal, normal_right);
                normalDiff += distance(centerNormal, normal_ur);
                normalDiff += distance(centerNormal, normal_ul);
                normalDiff += distance(centerNormal, normal_dr);
                normalDiff += distance(centerNormal, normal_dl);


                if (normalDiff > _NormalThreshold) {
                    return _EdgeColor;
                }
 else {
  return originalColor;
}
}
ENDCG
}
        }
}