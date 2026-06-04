shader_type spatial;
render_mode blend_mix,
	cull_back,
	depth_prepass_alpha,
	//shadows_disabled,
	//ambient_light_disabled,
	//vertex_lighting;
	specular_disabled;

uniform bool affine_mapping = false;
uniform vec4 albedo : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform sampler2D texture_albedo : source_color, filter_nearest;
uniform float brightness : hint_range(0, 1) = 0.5;
uniform float contrast : hint_range(0, 1) = 1.0;
uniform float alpha_scissor : hint_range(0, 1) = 0.5;
uniform float jitter: hint_range(0, 1) = 0.5;
uniform ivec2 resolution = ivec2(640, 360);

vec4 snap_to_position(vec4 base_position)
{
	vec4 snapped_position = base_position;
	snapped_position.xyz = base_position.xyz / base_position.w;

	vec2 snap_resolution = floor(vec2(resolution) * (1.0 - jitter));
	snapped_position.x = floor(snap_resolution.x * snapped_position.x) / snap_resolution.x;
	snapped_position.y = floor(snap_resolution.y * snapped_position.y) / snap_resolution.y;

	snapped_position.xyz *= base_position.w;
	return snapped_position;
}

void vertex()
{
	vec4 snapped_position = snap_to_position(PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0));
	if (affine_mapping)
	{
		POSITION = snapped_position;
		POSITION /= abs(POSITION.w);
	}
	else
	{
		POSITION = snapped_position;
	}
}

void fragment()
{
	vec4 averageLuminance = vec4(0.5, 0.5, 0.5, 1.0);
	vec4 texture_color = texture(texture_albedo, UV);
	vec3 fragRGB = texture_color.rgb + albedo.rgb;
	ALBEDO = mix(fragRGB * brightness, mix(averageLuminance.rgb, fragRGB, contrast), 0.5);

	ALPHA *= albedo.a * texture_color.a;
	ALPHA_SCISSOR_THRESHOLD = alpha_scissor;
}