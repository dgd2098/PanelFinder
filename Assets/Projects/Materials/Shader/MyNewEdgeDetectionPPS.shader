Shader "Hidden/Custom/MyNewEdgeDetectionPPS"
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

                // --- Unity 및 Post Processing 관련 필요한 매크로와 변수 수동 정의 ---
                // _CameraDepthNormalsTexture는 Depth Texture Mode가 DepthNormals일 때 Unity가 자동으로 채워줍니다.
                sampler2D _CameraDepthNormalsTexture;
                float4 _CameraDepthNormalsTexture_TexelSize; // 이 텍스처의 텍셀 크기 (오프셋 계산용)

                // Post Processing Stack v2는 이전 렌더 타겟을 _MainTex로 자동으로 바인딩합니다.
                sampler2D _MainTex;
                // sampler_MainTex 선언을 제거합니다. tex2D 함수를 직접 사용합니다.

                // 노멀 디코딩 함수 (UnityCG.cginc에서 가져옴)
                // View-space normal (RG) and packed depth (BA)
                float3 DecodeViewNormalStereo(float4 enc)
                {
                    float kScale = 1.7777;
                    float3 n;
                    n.xy = enc.xy * 2.0 - 1.0;
                    n.z = sqrt(1.0 - saturate(dot(n.xy, n.xy)));
                    n.xy *= kScale;
                    return n;
                }
                // --- End of Manual Definitions ---

                fixed4 _EdgeColor;
                float _NormalThreshold;
                float _EdgeWidth;

                // 버텍스 쉐이더 입력 구조체 (Post Processing Stack v2와 호환)
                struct VertInput {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                // 버텍스 쉐이더 출력 구조체
                struct VertOutput {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                VertOutput Vert(VertInput v) {
                    VertOutput o;
                    o.pos = UnityObjectToClipPos(v.vertex); // 기본 유니티 클립 공간 변환
                    o.uv = v.uv;
                    return o;
                }

                fixed4 Frag(VertOutput i) : SV_Target {
                    // !!! 수정된 부분: SAMPLE_TEXTURE2D_X 대신 tex2D 사용 !!!
                    fixed4 originalColor = tex2D(_MainTex, i.uv);

                // 현재 픽셀의 노멀 값 가져오기
                // !!! 수정된 부분: SAMPLE_TEXTURE2D_X 대신 tex2D 사용 !!!
                float4 normalDepth = tex2D(_CameraDepthNormalsTexture, i.uv);
                float3 centerNormal = DecodeViewNormalStereo(normalDepth);

                // 주변 픽셀 샘플링을 위한 텍셀 크기
                float2 texelSize = _CameraDepthNormalsTexture_TexelSize.xy;

                // 주변 픽셀의 노멀 값 샘플링 (8방향)
                // !!! 수정된 부분: SAMPLE_TEXTURE2D_X 대신 tex2D 사용 !!!
                float3 normal_up = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(0, _EdgeWidth) * texelSize));
                float3 normal_down = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(0, -_EdgeWidth) * texelSize));
                float3 normal_left = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(-_EdgeWidth, 0) * texelSize));
                float3 normal_right = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(_EdgeWidth, 0) * texelSize));

                float3 normal_ur = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(_EdgeWidth, _EdgeWidth) * texelSize));
                float3 normal_ul = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(-_EdgeWidth, _EdgeWidth) * texelSize));
                float3 normal_dr = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(_EdgeWidth, -_EdgeWidth) * texelSize));
                float3 normal_dl = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv + float2(-_EdgeWidth, -_EdgeWidth) * texelSize));


                // 노멀 벡터의 차이 합계 계산
                float normalDiff = 0;
                normalDiff += distance(centerNormal, normal_up);
                normalDiff += distance(centerNormal, normal_down);
                normalDiff += distance(centerNormal, normal_left);
                normalDiff += distance(centerNormal, normal_right);
                normalDiff += distance(centerNormal, normal_ur);
                normalDiff += distance(centerNormal, normal_ul);
                normalDiff += distance(centerNormal, normal_dr);
                normalDiff += distance(centerNormal, normal_dl);


                // 임계값을 넘으면 엣지 색상으로, 아니면 원본 색상으로
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