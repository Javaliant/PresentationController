package com.luigivincent.presentationcontroller;

/**
 * Created by Luigi on 10/8/2017.
 */

enum PebbleTrigger {
    UP_CLICK(0, "left"),
    SELECT_CLICK(1, "die"),
    DOWN_CLICK(2, "right");

    private final int key;
    private final String action;

    /**
     * @param key 		The actual data received from the pebble.
     * @param action 	The action indicated by the matching key.
     */
    PebbleTrigger(int key, String action) {
        this.key = key;
        this.action = action;
    }

    /**
     * Returns the PebbleTrigger constant's specified key.
     *
     * @return Key to represent and ascertain received.
     */
    public int key() {
        return key;
    }

    /**
     * Returns the PebbleTrigger constant's specified action.
     *
     * @return String to indicate which command matches trigger key.
     */
    public String action() {
        return action;
    }
}
