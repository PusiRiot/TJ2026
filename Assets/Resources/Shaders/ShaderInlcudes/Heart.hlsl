// The "_float" suffix is REQUIRED for Shader Graph to find it
void Heart_float(float2 UV, float Scale, out float Out)
{
    // Prevent division by zero errors on the GPU
    float safeScale = max(Scale, 0.0001);

    // Calculate the mathematical view size based on your scale
    float mathViewSize = 36.0 / safeScale;

    // 1. Scale and center the UVs
    float x = (UV.x - 0.5) * mathViewSize;
    // We leave the -3.5 offset unscaled so the heart stays perfectly centered!
    float y = (UV.y - 0.5) * mathViewSize - 3.5;

    // 2. Calculate U (sin^2(t))
    float U = pow(abs(x) + 0.00001, 0.66666667) / 6.3496;

    // 3. Build the implicit equation
    float term1 = y + 6.0 - 18.0 * U + 8.0 * U * U;
    float term2 = (1.0 - U) * pow(11.0 + 8.0 * U, 2.0);

    // 4. Combine and check if inside the heart
    float equation = (term1 * term1) - term2;

    // 5. Anti-aliasing for smooth edges
    float edgeThickness = fwidth(equation) * 1.5;
    Out = smoothstep(edgeThickness, -edgeThickness, equation);

}