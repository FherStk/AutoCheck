import java.util.*;
import java.io.*;

public class E1 {
  public static void main(String[] args) {

    Scanner sc = new Scanner(System.in);

    int hora = sc.nextInt();
    int minutos = sc.nextInt();
    int porcentaje = sc.nextInt();

    if (minutos == 0){
      System.out.println("OPEN");
    } else if (porcentaje < 50 && minutos <= 44) {
        System.out.println("OPEN");
    } else {
        System.out.println("CLOSE");
    }
  }
}
