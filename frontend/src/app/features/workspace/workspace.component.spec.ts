import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { WorkspaceComponent } from './workspace.component';
import { environment } from '../../../environments/environment';

describe('WorkspaceComponent', () => {
  let component: WorkspaceComponent;
  let fixture: ComponentFixture<WorkspaceComponent>;
  let http: HttpTestingController;
  let router: Router;

  /** Flush all the initial requests that fire in ngOnInit. */
  function flushInitialRequests(): void {
    // loadDashboard -> dashboardStats + loadActivity (by month)
    http.match(`${environment.businessApiUrl}/dashboard/stats`).forEach((req) =>
      req.flush({ success: true, data: mockStats(), message: '', errors: [] }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/orders-by-month`).forEach((req) =>
      req.flush({ success: true, data: [], message: '', errors: [] }),
    );
    // loadCustomers
    http.match(`${environment.businessApiUrl}/customers`).forEach((req) =>
      req.flush({ success: true, data: [], message: '', errors: [] }),
    );
    // loadOrders
    http.match((r) => r.url.includes('/orders')).forEach((req) =>
      req.flush({
        success: true,
        data: { items: [], pagination: { total: 0, per_page: 100, current_page: 1, last_page: 1 } },
        message: '',
        errors: [],
      }),
    );
  }

  function mockStats() {
    return {
      total_orders: 10,
      pending_orders: 3,
      in_progress_orders: 2,
      completed_orders: 4,
      cancelled_orders: 1,
      total_revenue: 500.0,
      total_customers: 5,
      active_customers: 4,
    };
  }

  beforeEach(async () => {
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [WorkspaceComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WorkspaceComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    fixture.detectChanges();

    flushInitialRequests();
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should start with dashboard view', () => {
    expect(component.view()).toBe('dashboard');
    expect(component.pageTitle()).toBe('Resumen');
  });

  it('should switch views correctly', () => {
    component.selectView('customers');
    expect(component.view()).toBe('customers');
    expect(component.pageTitle()).toBe('Clientes');

    component.selectView('orders');
    expect(component.view()).toBe('orders');
    expect(component.pageTitle()).toBe('Pedidos');

    // selectView('orders') triggers loadOrders again
    const ordersReq = http.expectOne((req) => req.url.includes('/orders'));
    ordersReq.flush({
      success: true,
      data: { items: [], pagination: { total: 0, per_page: 100, current_page: 1, last_page: 1 } },
      message: '',
      errors: [],
    });
  });

  it('should load dashboard stats on init', () => {
    expect(component.stats()).toEqual(mockStats());
  });

  it('should submit a new customer successfully', () => {
    component.selectView('customers');

    component.customerForm.patchValue({
      name: 'María López',
      email: 'maria@test.com',
      phone: '0991234567',
      address: 'Quito, Ecuador',
    });

    component.submitCustomer();
    expect(component.saving()).toBeTrue();

    // POST create customer
    const createReq = http.expectOne(`${environment.businessApiUrl}/customers`);
    expect(createReq.request.method).toBe('POST');
    createReq.flush({
      success: true,
      data: { id: 1, name: 'María López', email: 'maria@test.com', phone: '0991234567', address: 'Quito, Ecuador', is_active: true, created_at: '2026-09-29' },
      message: 'Cliente creado exitosamente',
      errors: [],
    });

    expect(component.saving()).toBeFalse();

    // After success, it reloads customers and dashboard
    http.match(`${environment.businessApiUrl}/customers`).forEach((req) =>
      req.flush({ success: true, data: [], message: '', errors: [] }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/stats`).forEach((req) =>
      req.flush({ success: true, data: mockStats(), message: '', errors: [] }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/orders-by-month`).forEach((req) =>
      req.flush({ success: true, data: [], message: '', errors: [] }),
    );
  });

  it('should not submit customer form when invalid', () => {
    component.customerForm.patchValue({ name: '', email: '' });
    component.submitCustomer();
    expect(component.saving()).toBeFalse();
  });

  it('should edit an existing customer by pre-filling the form', () => {
    const customer = {
      id: 5,
      name: 'Carlos Pérez',
      email: 'carlos@test.com',
      phone: '0997654321',
      address: 'Guayaquil',
      is_active: true,
      created_at: '2026-01-01',
    };

    component.editCustomer(customer);

    expect(component.editingCustomerId()).toBe(5);
    expect(component.customerForm.value.name).toBe('Carlos Pérez');
    expect(component.customerForm.value.email).toBe('carlos@test.com');
    expect(component.view()).toBe('customers');
  });

  it('should reset customer form correctly', () => {
    component.editingCustomerId.set(10);
    component.customerForm.patchValue({ name: 'Test' });

    component.resetCustomerForm();

    expect(component.editingCustomerId()).toBeNull();
    expect(component.customerForm.value.name).toBe('');
  });

  it('should submit a new order successfully', () => {
    component.customers.set([
      { id: 1, name: 'Test Client', email: 't@t.com', phone: null, address: null, is_active: true, created_at: '2026-01-01' },
    ]);

    component.orderForm.patchValue({ customer_id: 1, notes: 'Urgent' });
    component.orderItems.at(0).patchValue({ description: 'Widget', quantity: 2, unit_price: 25.5 });

    component.submitOrder();
    expect(component.saving()).toBeTrue();

    const createReq = http.expectOne(`${environment.businessApiUrl}/orders`);
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body.customer_id).toBe(1);
    expect(createReq.request.body.items.length).toBe(1);
    createReq.flush({
      success: true,
      data: {
        id: 1,
        customer_id: 1,
        status: 'Pending',
        total: 51,
        notes: 'Urgent',
        created_at: '2026-09-29',
        updated_at: '2026-09-29',
        items: [{ id: 1, description: 'Widget', quantity: 2, unit_price: 25.5 }],
      },
      message: '',
      errors: [],
    });

    expect(component.saving()).toBeFalse();

    // After success, it reloads orders and dashboard
    http.match((r) => r.url.includes('/orders')).forEach((req) =>
      req.flush({
        success: true,
        data: { items: [], pagination: { total: 0, per_page: 100, current_page: 1, last_page: 1 } },
        message: '',
        errors: [],
      }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/stats`).forEach((req) =>
      req.flush({ success: true, data: mockStats(), message: '', errors: [] }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/orders-by-month`).forEach((req) =>
      req.flush({ success: true, data: [], message: '', errors: [] }),
    );
  });

  it('should add and remove order items', () => {
    expect(component.orderItems.length).toBe(1);

    component.addOrderItem();
    expect(component.orderItems.length).toBe(2);

    component.removeOrderItem(0);
    expect(component.orderItems.length).toBe(1);

    // Should not go below 1 item
    component.removeOrderItem(0);
    expect(component.orderItems.length).toBe(1);
  });

  it('should transition an order to completed', () => {
    const order = {
      id: 7,
      customer_id: 1,
      status: 'Pending' as const,
      total: 100,
      notes: null,
      created_at: '2026-09-29',
      updated_at: '2026-09-29',
      items: [],
    };

    component.transitionOrder(order, 'complete');

    const req = http.expectOne(`${environment.businessApiUrl}/orders/7/complete`);
    expect(req.request.method).toBe('PATCH');
    req.flush({
      success: true,
      data: { ...order, status: 'Completed' },
      message: '',
      errors: [],
    });

    // Reloads orders + dashboard
    http.match((r) => r.url.includes('/orders')).forEach((r) =>
      r.flush({
        success: true,
        data: { items: [], pagination: { total: 0, per_page: 100, current_page: 1, last_page: 1 } },
        message: '',
        errors: [],
      }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/stats`).forEach((r) =>
      r.flush({ success: true, data: mockStats(), message: '', errors: [] }),
    );
    http.match(`${environment.businessApiUrl}/dashboard/orders-by-month`).forEach((r) =>
      r.flush({ success: true, data: [], message: '', errors: [] }),
    );
  });

  it('should format order status labels in Spanish', () => {
    expect(component.statusLabel('Pending')).toBe('Pendiente');
    expect(component.statusLabel('InProgress')).toBe('En curso');
    expect(component.statusLabel('Completed')).toBe('Completado');
    expect(component.statusLabel('Cancelled')).toBe('Cancelado');
  });

  it('should format currency in USD/Ecuador format', () => {
    const formatted = component.formatCurrency(1234.5);
    expect(formatted).toContain('1');
    expect(formatted).toContain('234');
  });

  it('should compute initials from session', () => {
    expect(component.initials()).toBeTruthy();
  });

  it('should compute activity bar heights correctly', () => {
    component.activityPoints.set([
      { date: '2026-09', count: 10, total: 100 },
      { date: '2026-08', count: 5, total: 50 },
      { date: '2026-07', count: 0, total: 0 },
    ]);

    expect(component.activityHeight({ date: '2026-09', count: 10, total: 100 })).toBe(100);
    expect(component.activityHeight({ date: '2026-08', count: 5, total: 50 })).toBe(50);
    expect(component.activityHeight({ date: '2026-07', count: 0, total: 0 })).toBe(8); // minimum
  });

  it('should filter customers by search query', () => {
    component.customers.set([
      { id: 1, name: 'Ana Torres', email: 'ana@test.com', phone: null, address: null, is_active: true, created_at: '2026-01-01' },
      { id: 2, name: 'Carlos López', email: 'carlos@test.com', phone: null, address: null, is_active: true, created_at: '2026-01-01' },
    ]);

    component.customerSearch.set('ana');
    expect(component.filteredCustomers().length).toBe(1);
    expect(component.filteredCustomers()[0].name).toBe('Ana Torres');

    component.customerSearch.set('');
    expect(component.filteredCustomers().length).toBe(2);
  });

  it('should reset order form correctly', () => {
    component.editingOrderId.set(99);
    component.addOrderItem();
    expect(component.orderItems.length).toBe(2);

    component.resetOrderForm();

    expect(component.editingOrderId()).toBeNull();
    expect(component.orderItems.length).toBe(1);
  });

  it('should logout and navigate to /login', () => {
    const navigateSpy = spyOn(router, 'navigateByUrl');
    component.logout();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });
});
