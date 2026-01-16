shader_type spatial;
render_mode blend_add,
	cull_disabled,
	depth_prepass_alpha,
	//shadows_disabled,
	//ambient_light_disabled,
	vertex_lighting,
	specular_disabled;

uniform bool affine_mapping = false;
uniform vec4 albedo : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform sampler2D texture_albedo : source_color, filter_nearest;
uniform float alpha_scissor : hint_range(0, 1) = 0.5;
uniform float jitter: hint_range(0, 1) = 0.5;
uniform ivec2 resolution = ivec2(640, 360);
uniform bool flashing = false;
	uniform vec4 flashColor : source_color = vec4(0.0, 0.5, 0.33, 1.0);
	uniform float flashSpeed = 4.0;
	uniform float flashIntensity = 2.0;
/** Whether to pass time via a script for scaled time */
uniform bool useExternalTime = false;
	uniform float timer = 0.0;

uniform float proximity_fade_distance : hint_range(0.0, 4096.0, 0.01) = 1.0;
uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;

vec4 snap_to_position(vec4 base_position)
{
	vec4 snapped_position = base_position;
	snapped_position.xyz = base_position.xyz / base_position.w;

	vec2 snap_resulotion = floor(vec2(resolution) * (1.0 - jitter));
	snapped_position.x = floor(snap_resulotion.x * snapped_position.x) / snap_resulotion.x;
	snapped_position.y = floor(snap_resulotion.y * snapped_position.y) / snap_resulotion.y;

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

float pingPong(float value, float maxV) //return value between 0 and 1
{
	return abs(fract((value - maxV) / (maxV * 2.0)) * maxV * 2.0 - maxV);
}

void fragment()
{
	vec4 texture_color = texture(texture_albedo, UV);
	if (flashing)
	{
		float alpha = 0.0;
		if (useExternalTime){
			alpha = pingPong(timer * flashSpeed, 1.0);
		}
		else	alpha = pingPong(TIME * flashSpeed, 1.0);
		
		ALBEDO = texture_color.rgb + albedo.rgb + (flashColor.rgb * (alpha * flashIntensity));	
		ALPHA *= texture_color.a * albedo.a;
		
		// Proximity Fade: Enabled
		float proximity_depth_tex = textureLod(depth_texture, SCREEN_UV, 0.0).r;
		vec4 proximity_view_pos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, proximity_depth_tex, 1.0);
		proximity_view_pos.xyz /= proximity_view_pos.w;
		ALPHA *= abs(clamp(alpha - smoothstep(proximity_view_pos.z + proximity_fade_distance, proximity_view_pos.z, VERTEX.z), 0.2, 1.0));
		
	}
	else 
	{ 
		ALBEDO = texture_color.rgb + albedo.rgb;
		ALPHA *= texture_color.a * albedo.a;
	}
	
	
	//ALPHA_SCISSOR_THRESHOLD = alpha_scissor;
}