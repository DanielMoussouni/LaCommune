shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_disabled;

uniform vec4 fire_color : source_color = vec4(1.0, 0.6, 0.1, 1.0);
uniform float radius = 0.2;
uniform float elongation = 2.0;
uniform float taper_factor = 0.8;
uniform float distortion_speed = 4.0;
uniform float distortion_freq = 8.0;
uniform float distortion_amp = 0.05;
uniform float emission_intensity = 3.0;

uniform float fps = 12.0;

void vertex() {
    // Lock vertically : billboard effect
    MODELVIEW_MATRIX = VIEW_MATRIX * mat4(
        vec4(normalize(cross(vec3(0.0, 1.0, 0.0), INV_VIEW_MATRIX[2].xyz)), 0.0), 
        vec4(0.0, 1.0, 0.0, 0.0), 
        vec4(normalize(cross(INV_VIEW_MATRIX[0].xyz, vec3(0.0, 1.0, 0.0))), 0.0), 
        MODEL_MATRIX[3]
    );
    
    MODELVIEW_MATRIX = MODELVIEW_MATRIX * mat4(
        vec4(length(MODEL_MATRIX[0].xyz), 0.0, 0.0, 0.0), 
        vec4(0.0, length(MODEL_MATRIX[1].xyz), 0.0, 0.0), 
        vec4(0.0, 0.0, length(MODEL_MATRIX[2].xyz), 0.0), 
        vec4(0.0, 0.0, 0.0, 1.0)
    );
    
    MODELVIEW_NORMAL_MATRIX = mat3(MODELVIEW_MATRIX);
}

void fragment() {
    vec2 center = vec2(0.5, 0.5);
    vec2 uv = UV;
    
    float chopped_time = floor(TIME * fps) / fps;
    
    float mask = 1.0 - uv.y; 
    
    uv.x += sin(uv.y * distortion_freq + chopped_time * distortion_speed) * distortion_amp * mask;
    uv.y += cos(uv.x * distortion_freq + chopped_time * distortion_speed) * (distortion_amp * 0.5) * mask;
    
    uv.y = (uv.y - center.y) / elongation + center.y;
    uv.x = (uv.x - center.x) / (1.0 - (1.0 - uv.y) * taper_factor) + center.x;

    float d = distance(uv, center);
    if (d < radius) {
        ALBEDO = fire_color.rgb;
        EMISSION = fire_color.rgb * emission_intensity;
        ALPHA = 1.0;
    } else {
        discard;
    }
}