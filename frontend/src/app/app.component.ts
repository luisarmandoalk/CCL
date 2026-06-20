import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

interface LoginResponse {
  token: string;
}

interface Producto {
  id: number;
  nombre: string;
  cantidad: number;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private readonly apiUrl = 'http://localhost:5000';
  token = '';
  cargando = false;
  mensaje = '';
  esError = false;
  inventario: Producto[] = [];
  loginForm!: any;
  movimientoForm!: any;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient
  ) {
    const tokenGuardado = localStorage.getItem('ccl_token');
    if (tokenGuardado !== null) {
      this.token = tokenGuardado;
    }

    this.loginForm = this.fb.group({
      usuario: ['admin', [Validators.required]],
      clave: ['admin123', [Validators.required]]
    });

    this.movimientoForm = this.fb.group({
      nombre: ['', [Validators.required]],
      cantidad: [1, [Validators.required, Validators.min(1)]],
      tipo: ['entrada', [Validators.required]]
    });
  }

  ngOnInit(): void {
    if (this.token !== '') {
      this.cargarInventario();
    }
  }

  get autenticado(): boolean {
    return this.token !== '';
  }

  login(): void {
    if (this.loginForm.invalid) {
      this.mostrarMensaje('Completa usuario y clave.', true);
      return;
    }

    const usuario = this.loginForm.value.usuario ?? '';
    const clave = this.loginForm.value.clave ?? '';

    this.cargando = true;
    this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, { usuario, clave })
      .subscribe({
        next: (respuesta) => {
          this.token = respuesta.token;
          localStorage.setItem('ccl_token', this.token);
          this.mostrarMensaje('Inicio de sesión correcto.', false);
          this.cargando = false;
          this.cargarInventario();
        },
        error: () => {
          this.mostrarMensaje('Credenciales inválidas.', true);
          this.cargando = false;
        }
      });
  }

  registrarMovimiento(): void {
    if (this.movimientoForm.invalid) {
      this.movimientoForm.markAllAsTouched();
      this.mostrarMensaje('Completa los campos del movimiento.', true);
      return;
    }

    const nombre = this.movimientoForm.value.nombre ?? '';
    const cantidad = this.movimientoForm.value.cantidad ?? 0;
    const tipo = this.movimientoForm.value.tipo ?? 'entrada';

    this.cargando = true;
    this.http.post(`${this.apiUrl}/productos/movimiento`, { nombre, cantidad, tipo }, this.autorizacion())
      .subscribe({
        next: () => {
          this.movimientoForm.reset({
            nombre: '',
            cantidad: 1,
            tipo: 'entrada'
          });
          this.mostrarMensaje('Movimiento registrado.', false);
          this.cargando = false;
          this.cargarInventario();
        },
        error: (error) => {
          const texto = error?.error;
          if (typeof texto === 'string') {
            this.mostrarMensaje(texto, true);
          } else {
            this.mostrarMensaje('No se pudo registrar el movimiento.', true);
          }
          this.cargando = false;
        }
      });
  }

  cargarInventario(): void {
    if (this.token === '') {
      return;
    }

    this.http.get<Producto[]>(`${this.apiUrl}/productos/inventario`, this.autorizacion())
      .subscribe({
        next: (productos) => {
          this.inventario = productos;
        },
        error: () => {
          this.mostrarMensaje('No se pudo cargar el inventario.', true);
        }
      });
  }

  cerrarSesion(): void {
    this.token = '';
    localStorage.removeItem('ccl_token');
    this.inventario = [];
    this.loginForm.reset({ usuario: 'admin', clave: 'admin123' });
    this.mostrarMensaje('Sesión cerrada.', false);
  }

  private autorizacion() {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${this.token}`
    });

    return { headers };
  }

  private mostrarMensaje(texto: string, error: boolean): void {
    this.mensaje = texto;
    this.esError = error;
  }
}
