/* Author: Luigi Vincent
* Test Client for Presentation Controller
*/

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.Scanner;

public class TestClient {
	private final static String ADDRESS = "127.0.0.1";
	private final static int PORT = 9001;

	public static void main(String[] args) throws Exception {
		Scanner keyboard = new Scanner(System.in);
		Socket socket = new Socket(ADDRESS, PORT);

		BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
		PrintWriter out = new PrintWriter(socket.getOutputStream(), true);

		new Thread(() -> {
			String input = null;
			while (true) {
				try {
					if ((input = in.readLine()) != null) {
						System.out.println(input);
					}
				} catch (IOException ioe) { /* ignored for now */ }
			}
		}).start();

		while (true) {
			String output = keyboard.nextLine();
			out.print(output);
			out.flush();
			if (output.equals("die")) {
				System.exit(0);
			}
		}
	}
}