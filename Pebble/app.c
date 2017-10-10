#include <pebble.h>

#define UP_BUTTON_CLICK 0
#define SELECT_BUTTON_CLICK 1
#define DOWN_BUTTON_CLICK 2

static Window *window;
static TextLayer *text_layer;
static int active = 1;

static void send(int key, int value) {
	if (active) {
		DictionaryIterator *iter;
		app_message_outbox_begin(&iter);
		dict_write_int(iter, key, &value, sizeof(int), true);
		app_message_outbox_send();
	}
}

static void handle_up_click() {
	send(UP_BUTTON_CLICK, 0);
}

static void handle_select_click() {
	send(SELECT_BUTTON_CLICK, 0);
}

static void handle_down_click() {
	send(DOWN_BUTTON_CLICK, 0);
}

static void handle_back_click() {
	active = 1 - active;
	text_layer_set_text(text_layer, active ? "active" : "inactive");
}

static void outbox_failed_handler(DictionaryIterator *iter, AppMessageResult reason, void *context) {
	text_layer_set_text(text_layer, "Connection Failure.");
	APP_LOG(APP_LOG_LEVEL_ERROR, "Fail reason: %d", (int) reason);
}

static void configure_click_handlers() {
	window_single_click_subscribe(BUTTON_ID_UP, handle_up_click);
	window_single_click_subscribe(BUTTON_ID_SELECT, handle_select_click);
	window_single_click_subscribe(BUTTON_ID_DOWN, handle_down_click);
	window_single_click_subscribe(BUTTON_ID_BACK, handle_back_click);
}

static void load(Window *window) {
	window_set_background_color(window, GColorBlack);
	window_set_click_config_provider(window, configure_click_handlers);
	Layer *window_layer = window_get_root_layer(window);
	GRect bounds = layer_get_bounds(window_layer);
	
	text_layer = text_layer_create(GRect(0, PBL_IF_ROUND_ELSE(58, 52), bounds.size.w, 50));
	text_layer_set_text(text_layer, "Presentation Control!");
	text_layer_set_background_color(text_layer, GColorBlack);
	text_layer_set_text_color(text_layer, GColorRajah);
	text_layer_set_text_alignment(text_layer, GTextAlignmentCenter);
	layer_add_child(window_layer, text_layer_get_layer(text_layer));
}

static void unload(Window *window) {
	text_layer_destroy(text_layer);
}

static void init(void) {
	window = window_create();
	window_set_window_handlers(window, (WindowHandlers) {
		.load = load,
		.unload = unload
	});
	window_stack_push(window, true);

	app_message_register_outbox_failed(outbox_failed_handler);

	const int inbox_size = 128;
	const int outbox_size = 128;
	app_message_open(inbox_size, outbox_size);
}

static void deinit(void) {
	window_destroy(window);
}

int main(void) {
	init();
	app_event_loop();
	deinit();
}