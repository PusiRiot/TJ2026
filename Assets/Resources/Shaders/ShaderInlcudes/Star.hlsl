// The "_float" suffix is REQUIRED for Shader Graph to find it
void Star_float(float2 UV, float Scale, out float Out)
{
    // Prevent division by zero errors on the GPU
    float safeScale = max(Scale, 0.0001);

    // 1. Center the UVs (0.5 is the middle)
    // We multiply by 2.2 to give it a 10% padding so the points don't clip at Scale = 1.
    float2 p = (UV - 0.5) * (2.2 / safeScale);

    // 2. Setup Symmetry Constants for a 5-pointed star
    // k1 and k2 represent the angles (Pi/5) needed to mirror/fold the space
    float2 k1 = float2(0.809016994375, -0.587785252292);
    float2 k2 = float2(-k1.x, k1.y);

    // 3. Fold the space to create 5 repeating segments
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);

    // 4. Define Star Proportions
    float outerRadius = 1.0;

    // TIP: 0.5 gives a classic "Super Mario" star. 
    // Change to 0.382 if you want a perfectly straight-edged, sharp pentagram!
    float innerRadiusRatio = 0.5;

    // Shift origin to the top point of the star
    p.y -= outerRadius;

    // 5. Calculate the distance field of the star shape
    float2 ba = innerRadiusRatio * float2(-k1.y, k1.x) - float2(0.0, 1.0);
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, outerRadius);

    // equation will be Negative INSIDE the star, and Positive OUTSIDE
    float equation = length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);

    // 6. Anti-aliasing for smooth edges
    // fwidth tells us exactly how thick one pixel is on the screen
    float edgeThickness = fwidth(equation) * 1.5;

    // Flip the gradient so negative (inside) becomes 1.0 (white solid color)
    Out = smoothstep(edgeThickness, -edgeThickness, equation);
}