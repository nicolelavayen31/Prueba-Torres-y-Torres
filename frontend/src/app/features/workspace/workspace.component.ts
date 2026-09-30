import { CommonModule } from '@angular/common';
import { Component, computed, OnInit, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { BusinessApiService } from '../../core/services/business-api.service';
import { ActivityPoint, Customer, CustomerOrder, DashboardStats, OrderItem, OrderStatus } from '../../core/models/api.models';

type WorkspaceView = 'dashboard' | 'customers' | 'orders';
type OrderItemForm = FormGroup<{
  description: FormControl<string>;
  quantity: FormControl<number>;
  unit_price: FormControl<number>;
}>;

@Component({
  selector: 'app-workspace',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTooltipModule,
    RouterLink,
  ],
  templateUrl: './workspace.component.html',
  styleUrl: './workspace.component.scss',
})
export class WorkspaceComponent implements OnInit {
  readonly today = new Date();
  readonly view = signal<WorkspaceView>('dashboard');
  readonly customers = signal<Customer[]>([]);
  readonly orders = signal<CustomerOrder[]>([]);
  readonly stats = signal<DashboardStats | null>(null);
  readonly activityPoints = signal<ActivityPoint[]>([]);
  readonly activityPeriod = signal<'day' | 'month'>('month');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editingCustomerId = signal<number | null>(null);
  readonly editingOrderId = signal<number | null>(null);
  readonly pageTitle = computed(() => ({
    dashboard: 'Resumen',
    customers: 'Clientes',
    orders: 'Pedidos',
  })[this.view()]);
  readonly activeCustomerCount = computed(() => this.customers().filter((customer) => customer.is_active).length);
  readonly customerSearch = signal('');
  readonly filteredCustomers = computed(() => {
    const query = this.customerSearch().trim().toLocaleLowerCase();
    return this.customers().filter((customer) =>
      `${customer.name} ${customer.email} ${customer.phone ?? ''}`.toLocaleLowerCase().includes(query),
    );
  });

  readonly customerForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    phone: ['', [Validators.maxLength(20)]],
    address: ['', [Validators.maxLength(300)]],
  });

  readonly orderItems = this.formBuilder.array([this.createOrderItemForm()]);
  readonly orderForm = this.formBuilder.nonNullable.group({
    customer_id: [0, [Validators.required, Validators.min(1)]],
    notes: ['', [Validators.maxLength(500)]],
    items: this.orderItems,
  });

  readonly orderFilterForm = this.formBuilder.nonNullable.group({
    status: [''],
    customer_id: [''],
    date_from: [''],
    date_to: [''],
  });

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly api: BusinessApiService,
    readonly auth: AuthService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
    this.loadCustomers();
    this.loadOrders();
  }

  selectView(view: WorkspaceView): void {
    this.view.set(view);
    if (view === 'orders') {
      this.loadOrders();
    }
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.api.dashboardStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.showError(error);
      },
    });
    this.loadActivity();
  }

  setActivityPeriod(period: string): void {
    if (period !== 'day' && period !== 'month') {
      return;
    }
    this.activityPeriod.set(period);
    this.loadActivity();
  }

  private loadActivity(): void {
    const activityRequest = this.activityPeriod() === 'day'
      ? this.api.activityByDay()
      : this.api.activityByMonth();
    activityRequest.subscribe({
      next: (points) => this.activityPoints.set(Array.isArray(points) ? points.slice(0, 12).reverse() : []),
      error: (error) => this.showError(error),
    });
  }

  loadCustomers(): void {
    this.api.customers().subscribe({
      next: (customers) => this.customers.set(customers),
      error: (error) => this.showError(error),
    });
  }

  loadOrders(): void {
    const filters = this.orderFilterForm.getRawValue();
    this.api.orders({ ...filters, per_page: 100 }).subscribe({
      next: (page) => this.orders.set(page.items),
      error: (error) => this.showError(error),
    });
  }

  submitCustomer(): void {
    if (this.customerForm.invalid || this.saving()) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const id = this.editingCustomerId();
    const request = id
      ? this.api.updateCustomer(id, this.customerForm.getRawValue())
      : this.api.createCustomer(this.customerForm.getRawValue());

    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.resetCustomerForm();
        this.loadCustomers();
        this.loadDashboard();
        this.notify(id ? 'Cliente actualizado.' : 'Cliente creado.');
      },
      error: (error) => {
        this.saving.set(false);
        this.showError(error);
      },
    });
  }

  editCustomer(customer: Customer): void {
    this.editingCustomerId.set(customer.id);
    this.customerForm.patchValue({
      name: customer.name,
      email: customer.email,
      phone: customer.phone ?? '',
      address: customer.address ?? '',
    });
    this.selectView('customers');
  }

  archiveCustomer(customer: Customer): void {
    if (!window.confirm(`¿Desactivar a ${customer.name}?`)) {
      return;
    }

    this.api.archiveCustomer(customer.id).subscribe({
      next: () => {
        this.loadCustomers();
        this.loadDashboard();
        this.notify('Cliente desactivado.');
      },
      error: (error) => this.showError(error),
    });
  }

  resetCustomerForm(): void {
    this.editingCustomerId.set(null);
    this.customerForm.reset({ name: '', email: '', phone: '', address: '' });
  }

  submitOrder(): void {
    if (this.orderForm.invalid || this.saving()) {
      this.orderForm.markAllAsTouched();
      return;
    }

    const value = this.orderForm.getRawValue();
    const payload = {
      customer_id: Number(value.customer_id),
      notes: value.notes || undefined,
      items: value.items.map((item) => ({
        description: item.description,
        quantity: Number(item.quantity),
        unit_price: Number(item.unit_price),
      })),
    };
    const orderId = this.editingOrderId();
    this.saving.set(true);
    const request = orderId
      ? this.api.updateOrder(orderId, payload)
      : this.api.createOrder(payload);

    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.resetOrderForm();
        this.loadOrders();
        this.loadDashboard();
        this.notify(orderId ? 'Pedido actualizado.' : 'Pedido creado.');
      },
      error: (error) => {
        this.saving.set(false);
        this.showError(error);
      },
    });
  }

  editOrder(order: CustomerOrder): void {
    this.editingOrderId.set(order.id);
    this.orderItems.clear();
    for (const item of order.items) {
      this.orderItems.push(this.createOrderItemForm(item));
    }
    if (this.orderItems.length === 0) {
      this.orderItems.push(this.createOrderItemForm());
    }
    this.orderForm.patchValue({ customer_id: order.customer_id, notes: order.notes ?? '' });
    this.selectView('orders');
  }

  addOrderItem(): void {
    this.orderItems.push(this.createOrderItemForm());
  }

  removeOrderItem(index: number): void {
    if (this.orderItems.length > 1) {
      this.orderItems.removeAt(index);
    }
  }

  resetOrderForm(): void {
    this.editingOrderId.set(null);
    this.orderItems.clear();
    this.orderItems.push(this.createOrderItemForm());
    this.orderForm.reset({ customer_id: 0, notes: '' });
  }

  transitionOrder(order: CustomerOrder, action: 'complete' | 'cancel'): void {
    this.api.changeOrderStatus(order.id, action).subscribe({
      next: () => {
        this.loadOrders();
        this.loadDashboard();
        this.notify(action === 'complete' ? 'Pedido completado.' : 'Pedido cancelado.');
      },
      error: (error) => this.showError(error),
    });
  }

  deleteOrder(order: CustomerOrder): void {
    if (!window.confirm(`¿Eliminar el pedido #${order.id}?`)) {
      return;
    }

    this.api.deleteOrder(order.id).subscribe({
      next: () => {
        this.loadOrders();
        this.loadDashboard();
        this.notify('Pedido eliminado.');
      },
      error: (error) => this.showError(error),
    });
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }

  initials(): string {
    const displayName = this.auth.session()?.displayName ?? this.auth.session()?.email ?? 'OF';
    return displayName.split(/[\s@.]+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join('').toUpperCase();
  }

  statusLabel(status: OrderStatus): string {
    return {
      Pending: 'Pendiente',
      InProgress: 'En curso',
      Completed: 'Completado',
      Cancelled: 'Cancelado',
    }[status];
  }

  activityHeight(point: ActivityPoint): number {
    const max = Math.max(...this.activityPoints().map((entry) => entry.count), 1);
    return Math.max(8, Math.round((point.count / max) * 100));
  }

  activityLabel(value: string): string {
    const dateValue = this.activityPeriod() === 'day' ? value : `${value}-01`;
    const date = new Date(`${dateValue}T00:00:00`);
    return this.activityPeriod() === 'day'
      ? date.toLocaleDateString('es-EC', { day: '2-digit', month: '2-digit' })
      : date.toLocaleDateString('es-EC', { month: 'short' }).replace('.', '');
  }

  formatCurrency(value: number | null | undefined): string {
    return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD', maximumFractionDigits: 2 }).format(value ?? 0);
  }

  private showError(error: { error?: { message?: string; errors?: string[] }; message?: string }): void {
    const message = error.error?.errors?.join(' ') || error.error?.message || error.message || 'No fue posible completar la operación.';
    this.snackBar.open(message, 'Cerrar', { duration: 5000, panelClass: ['toast-error'] });
  }

  private notify(message: string): void {
    this.snackBar.open(message, 'Cerrar', { duration: 2800, panelClass: ['toast-success'] });
  }

  private createOrderItemForm(item?: Partial<OrderItem>): OrderItemForm {
    return this.formBuilder.nonNullable.group({
      description: [item?.description ?? '', [Validators.required, Validators.maxLength(300)]],
      quantity: [item?.quantity ?? 1, [Validators.required, Validators.min(1)]],
      unit_price: [item?.unit_price ?? 0, [Validators.required, Validators.min(0)]],
    }) as OrderItemForm;
  }
}